namespace Compiler.Handlers;

using System.Runtime.InteropServices;
using Runtime.Core;
using Data;

/// <summary>
/// This class manages the memory space of the program data section at compile time.
/// Objects which can exist in the data section can be stored and their address can be retrieved.
/// </summary>
internal sealed unsafe class DataHandler {
    /// <summary>
    /// The limit for the size of the data section is approximately 2 gigabytes.
    /// It is a reasonable assumption that we will not need more.
    /// </summary>
    private int DataLength { get; set; }

    /// <summary>
    /// Allocating objects that have a size not divisible by the rounding factor will leave holes in the data array.
    /// Allow storing small objects in these holes while still aligning as many objects as possible.
    /// </summary>
    private List<Hole> Holes { get; }

    // primitive types and strings can always be stored
    // keep their values in memory during compile time to avoid allocating memory for the same object twice
    // unsigned and signed integers have the same binary representation
    // so using a single collection is enough for a given size

    /// <summary>
    /// Collection of 8-bit integers.
    /// </summary>
    public IObjectCollection<I8> I8 { get; }

    /// <summary>
    /// Collection of 16-bit integers and unicode characters.
    /// </summary>
    public IObjectCollection<I16> I16 { get; }

    /// <summary>
    /// Collection of 32-bit integers.
    /// </summary>
    public IObjectCollection<I32> I32 { get; }

    /// <summary>
    /// Collection of 64-bit integers.
    /// </summary>
    public IObjectCollection<I64> I64 { get; }
    
    /// <summary>
    /// Collection of 128-bit integers.
    /// </summary>
    public IObjectCollection<I128> I128 { get; }

    /// <summary>
    /// Collection of 16-bit floating-point numbers.
    /// </summary>
    public IObjectCollection<F16> F16 { get; }

    /// <summary>
    /// Collection of 32-bit floating-point numbers.
    /// </summary>
    public IObjectCollection<F32> F32 { get; }

    /// <summary>
    /// Collection of 64-bit floating-point numbers.
    /// </summary>
    public IObjectCollection<F64> F64 { get; }

    /// <summary>
    /// Collection of booleans.
    /// </summary>
    public IObjectCollection<Bool> Bool { get; }

    /// <summary>
    /// Collection of strings.
    /// </summary>
    public IObjectCollection<string> Str { get; }

    /// <summary>
    /// Create a new data handler with no stored data.
    /// </summary>
    public DataHandler() {
        DataLength = 0;
        Holes = [];

        I8 = new PrimitiveCollection<I8>(this);
        I16 = new PrimitiveCollection<I16>(this);
        I32 = new PrimitiveCollection<I32>(this);
        I64 = new PrimitiveCollection<I64>(this);
        I128 = new PrimitiveCollection<I128>(this);
        F16 = new PrimitiveCollection<F16>(this);
        F32 = new PrimitiveCollection<F32>(this);
        F64 = new PrimitiveCollection<F64>(this);
        Bool = new PrimitiveCollection<Bool>(this);
        Str = new StringCollection(this);
    }

    /// <summary>
    /// Search for the optimal place for an object in memory.
    /// The type of the object is not relevant, only its size.
    /// </summary>
    /// <param name="size">The size of the object in bytes.</param>
    /// <returns>The assigned address of the object.</returns>
    private int AddObject(int size) {
        // the block size for aligning objects
        const int ROUNDING_FACTOR = 4;
        
        int address;

        // more complicated size: object size is not divisible by the rounding factor
        if (size % ROUNDING_FACTOR != 0) {
            // check if the object fits in any existing holes
            // the hole size is always smaller than the rounding factor,
            // so check only if the object is small enough
            if (size < ROUNDING_FACTOR) {
                for (int i = 0; i < Holes.Count; i++) {
                    // object too big
                    if (Holes[i].Size < size) continue;

                    // record the address to return
                    address = Holes[i].Address;

                    // update hole size and offset
                    Hole hole = new(Holes[i].Address + size, Holes[i].Size - size);

                    // no remaining space, simply remove from the list
                    if (hole.Size < 1) {
                        Holes.RemoveAt(i);
                    }

                    // still some space, replace item in the list
                    else {
                        Holes[i] = hole;
                    }

                    return address;
                }
            }

            // record the address to return
            address = DataLength;

            // round up object size
            int roundedSize = RoundUp(size, ROUNDING_FACTOR);

            // create a hole that is directly after the object
            Holes.Add(new Hole(address + size, roundedSize - size));

            // update the size
            DataLength += roundedSize;

            return address;
        }

        // simple case: object size is divisible by the rounding factor
        // object will be aligned and leave no holes in memory
        address = DataLength;
        
        // update the size
        DataLength += size;

        return address;
    }

    /// <summary>
    /// Create a byte array containing all objects stored by this data handler.
    /// </summary>
    /// <returns>The created byte array.</returns>
    public byte[] ToBytes() {
        int length = RoundUp(DataLength, 16);
        byte[] bytes = new byte[length];

        I8.WriteContents(bytes);
        I16.WriteContents(bytes);
        I32.WriteContents(bytes);
        I64.WriteContents(bytes);
        I128.WriteContents(bytes);
        F16.WriteContents(bytes);
        F32.WriteContents(bytes);
        F64.WriteContents(bytes);
        Bool.WriteContents(bytes);
        Str.WriteContents(bytes);

        return bytes;
    }

    /// <summary>
    /// Round up the integer by a given factor. It is assumed both numbers are positive.
    /// </summary>
    /// <param name="value">The number to round up.</param>
    /// <param name="factor">the rounding factor.</param>
    /// <returns>The rounded-up value.</returns>
    private static int RoundUp(int value, int factor) {
        return (value + factor - 1) / factor * factor;
    }

    /// <summary>
    /// An interface containing all the methods that must be implemented
    /// by a collection storing objects in the data section.
    /// </summary>
    /// <typeparam name="T">The type of the stored object.</typeparam>
    public interface IObjectCollection<in T> {
        /// <summary>
        /// Store an object in the collection.
        /// </summary>
        /// <param name="value">The object to store.</param>
        /// <returns>The address of the object in the data section.</returns>
        public MemoryAddress Add(T value);
        
        /// <summary>
        /// Write all stored objects to a byte array using their memory addresses.
        /// </summary>
        /// <param name="bytes">The destination array.</param>
        public void WriteContents(byte[] bytes);
    }

    /// <summary>
    /// The simplest implementation of IObjectCollection for unmanaged types.
    /// </summary>
    /// <param name="dataHandler">The data handler to use.</param>
    /// <typeparam name="T">The type of the stored object.</typeparam>
    private sealed class PrimitiveCollection<T>(DataHandler dataHandler) : IObjectCollection<T> where T : unmanaged {
        /// <summary>
        /// To determine the actual address of an object, the current DataHandler instance must be used.
        /// </summary>
        private DataHandler DataHandler { get; } = dataHandler;
        
        /// <summary>
        /// Store object-address pairs in a dictionary.
        /// This allows for duplicate-checks and returning the address of duplicates.
        /// </summary>
        private Dictionary<T, int> Objects { get; } = [];
        
        public MemoryAddress Add(T value) {
            // check if the object is already stored
            bool contains = Objects.TryGetValue(value, out int address);

            // return the stored address of duplicates
            if (contains) {
                return MemoryAddress.CreateInData(address);
            }

            // determine object address
            // use unmanaged size 
            address = DataHandler.AddObject(sizeof(T));

            // store the object
            Objects.Add(value, address);

            return MemoryAddress.CreateInData(address);;
        }
        
        public void WriteContents(byte[] bytes) {
            // simple foreach through all objects
            foreach (KeyValuePair<T, int> pair in Objects) {
                // create a span starting at the object address
                Span<byte> destination = bytes.AsSpan(pair.Value);
                
                // this method can be used only because T is unmanaged
                MemoryMarshal.Write(destination, pair.Key);
            }
        }
    }

    /// <summary>
    /// An implementation of IObjectCollection for strings.
    /// </summary>
    /// <param name="dataHandler">The data handler to use.</param>
    private sealed class StringCollection(DataHandler dataHandler) : IObjectCollection<string> {
        /// <summary>
        /// To determine the actual address of an object, the current DataHandler instance must be used.
        /// </summary>
        private DataHandler DataHandler { get; } = dataHandler;
        
        /// <summary>
        /// Store object-address pairs in a dictionary.
        /// This allows for duplicate-checks and returning the address of duplicates.
        /// </summary>
        private Dictionary<string, int> Objects { get; } = [];

        public MemoryAddress Add(string value) {
            // check if the object is already stored
            bool contains = Objects.TryGetValue(value, out int address);

            // return the stored address of duplicates
            // the way strings are stored in memory only allows the optimisation for duplicates,
            // if the 2 strings are exactly the same
            if (contains) {
                return MemoryAddress.CreateInData(address);;
            }

            // determine object address
            // the length of the string depend on the way its stored
            address = DataHandler.AddObject(sizeof(int) + value.Length * sizeof(char));

            // store the object
            Objects.Add(value, address);

            return MemoryAddress.CreateInData(address);;
        }

        public void WriteContents(byte[] bytes) {
            // simple foreach through all objects
            foreach (KeyValuePair<string, int> pair in Objects) {
                // create a span starting at the object address
                Span<byte> destination = bytes.AsSpan(pair.Value);

                // store the length of the string as a 32-bit integer
                MemoryMarshal.Write(destination, pair.Key.Length);

                // cast the characters into a span of bytes
                ReadOnlySpan<byte> characterBytes = MemoryMarshal.Cast<char, byte>(pair.Key);

                // store the characters of the string after the length
                characterBytes.CopyTo(destination[sizeof(int)..]);
            }
        }
    }

    /// <summary>
    /// A simple class to represent a small region of memory.
    /// This memory is free but unaligned, so small objects can still be positioned here.
    /// </summary>
    /// <param name="address">The starting memory address.</param>
    /// <param name="size">The size in bytes.</param>
    [StructLayout(LayoutKind.Sequential)]
    private readonly struct Hole(int address, int size) {
        /// <summary>
        /// The starting memory address.
        /// </summary>
        public readonly int Address = address;
        
        /// <summary>
        /// The size in bytes.
        /// </summary>
        public readonly int Size = size;
    }
}