# Nyelv specifikációi

## Szintaxis

### Egy program felépítése
Egy program "import" utasításokkal kezdődhet, melyek célja jelezni az értelmezőnek, hogy az adott modulból használunk kódot. Rendelkezésünkre áll az automatikus importálás használata is, amivel az értelmező minden elérhető modulban keresi az szükséges osztályokat, és importálja azt, amely szükséges is. Ez természetesen időigényes, illetve egyes nevek ütközéséhez vezethet. Az automatikus importálás nem zárja ki több import utasítás használatát.
```
import <modulnév>
import auto
```
Ez után helyezkedhetnek el az "in" és "out" paraméterek, tetszőleges sorrendben. Az "in" paraméterek a program bemeneti értékei, az "out" paraméterek visszatérési értékei. Mindkét esetben egy függvény fejlécéhez hasonlóan kell felsorolni a változók típusait és neveit. Mindkét sor külön-külön elhagyható, illetve a "null" kifejezéssel expliciten megadható, hogy nincs ilyen paraméter. Ha a program indulásakor egy bemeneti paraméter nem kap értéket, vagy a kapott érték nem megfelelő típusú, a változó NULL lesz. Minden kimeneti paraméter kezdeti értéke NULL. 
```
in  <típus> <változónév>, <típus> <változónév>, ...
out <típus> <változónév>, <típus> <változónév>, ...
```
```
in  null
out null
```
