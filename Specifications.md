# Nyelv specifikációi

## Szintaxis

### Egy program felépítése
Egy program `import` utasításokkal kezdődhet, melyek célja jelezni az értelmezőnek, hogy az adott modulból használunk kódot. Rendelkezésünkre áll az automatikus importálás használata is, amivel az értelmező minden elérhető modulban keresi az szükséges osztályokat, és importálja azt, amely szükséges is. Ez természetesen időigényes, illetve egyes nevek ütközéséhez vezethet. Az automatikus importálás nem zárja ki több import utasítás használatát.
```
import <modulnév>
import auto
```
<br> Ez után helyezkedhetnek el az `in` és `out` paraméterek, tetszőleges sorrendben. Az `in` paraméterek a program bemeneti értékei, az `out` paraméterek visszatérési értékei. Mindkét esetben egy függvény fejlécéhez hasonlóan kell felsorolni a változók típusait és neveit. <br><br> Mindkét sor külön-külön elhagyható, illetve a `null` kifejezéssel expliciten megadható, hogy nincs ilyen paraméter. Ha a program indulásakor egy bemeneti paraméter nem kap értéket, vagy a kapott érték nem megfelelő típusú, a változó `null` lesz. Minden kimeneti paraméter kezdeti értéke `null`. 
```
in  <típus> <változónév>, <típus> <változónév>, ...
out <típus> <változónév>, <típus> <változónév>, ...

in  null
out null
```
<br> Ezt követően helyezkedik el bármilyen más programkód.

### Típusok

#### Előjeles egész számok
Rövidített nevük az "i", mint "integer" prefix, amit a bitek száma követ. Így létezik `i8`, `i16`, `i32` és `i64`.
```
i32 x = -1;
i64 y = 2;
```
#### Előjeltelen egész számok
Rövidített nevük az "u", mint "unsigned" prefix, amit a bitek száma követ. Így létezik `u8`, `u16`, `u32` és `u64`.
```
u32 x = 1;
u64 y = 2;
```
#### Lebegőpontos számok
Rövidített nevük az "f", mint "float" prefix, amit a bitek száma követ. Így létezik `f16`, `f32` és `f64`.
```
f32 x = 0.1234;
f64 y = 3.1415927;
```
#### Karakterek


### Változók
Minden típus ősosztálya (közvetlenül vagy közvetetten) az `object`. Minden típus referenciatípus, azaz paraméterben való átadáskor, vagy függvényből való visszatéréskor egy változóról nem készül másolat. Ennek köszönhetően minden változó értéke lehet `null` is.
Egy változó létrehozásakor kötelező azt egy típussal ellátni, értéket adni viszont opcionális, melynek hiányában `null` lesz a kezdőérték.
```
<típus> <név>;
<típus> <név> = <érték>;
```
A változókra azonosítókkal tudunk hivatkozni. Ezek számokat (0-9), kis- és nagybetűket (a-z és A-Z) és alsóvonásokat (_) tartalmazhatnak azzal a kitétellel, hogy az első karakter nem lehet szám. Például a `_name` és a `_0` azonosítók helyesek, viszont a `2name` nem.
