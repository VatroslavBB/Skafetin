# Škafetin - e-Imovina Županije

Aplikacija za evidenciju imovine županije: popis opreme po lokacijama i kategorijama, zaduživanje opreme zaposlenicima s cjelovitom poviješću, inventure po lokacijama sa stavkama i odstupanjima, zahtjevi zaposlenika za opremom, postupak otpisa te slike i dokumenti uz opremu. Pristup je moguć tek nakon prijave, a što korisnik vidi i smije ovisi o njegovim ulogama.

## Tehnologije

.NET 10, ASP.NET Core Web API, Blazor Server s MudBlazor, EF Core i SQLite, JWT autentifikacija.

| Projekt | Uloga |
|---|---|
| `Skafetin.Api` | Web API, baza, autentifikacija i autorizacija, Swagger |
| `Skafetin.App` | Blazor Server korisničko sučelje |
| `Skafetin.Shared` | Modeli i DTO klase koje koriste oba projekta |

## Što aplikacija radi

Škafetin prati imovinu kroz njezin životni ciklus: od unosa u evidenciju, preko zaduživanja zaposlenicima i redovnih inventura, do otpisa. Svaka promjena ostavlja trag, pa se za bilo koji komad opreme može rekonstruirati gdje je bio, kod koga i u kakvom stanju.

### Oprema

Središnja evidencija. Zapis nosi jedinstven inventurni broj, naziv, kategoriju, status, lokaciju te neobavezno proizvođača, model, serijski broj, opis i nabavnu vrijednost.

Popis (`/equipment`) ima pretragu, četiri filtera (kategorija, status, lokacija, zaposlenik), sortiranje po stupcima, stranicanje i gumb za čišćenje filtara. Sve to radi na API strani, nad upitom prema bazi, pa preglednik dobiva samo traženu stranicu.

Profil opreme (`/equipment/{id}`) objedinjuje osnovne podatke, trenutno i prošla zaduženja, povijest statusa, priložene slike i dokumente te QR naljepnicu.

Statusi: `Na skladištu`, `Zaduženo`, `Na servisu`, `Nedostaje`, `Otpisano`. Status se ne uređuje ručno kroz obrazac nego se mijenja radnjama - zaduženjem, povratom, otpisom - a svaka promjena upisuje redak u `EquipmentStatusHistory`.

Osim uređivanja, oprema se može premjestiti na drugu lokaciju (`POST /api/equipment/{id}/move`). Brisanje postoji, ali je ograničeno na `Admin` i namijenjeno ispravku pogrešnog unosa, ne otpisu.

### Zaduženja

Zaduženje povezuje opremu i zaposlenika kroz vremenski period. Statusi: `Aktivno`, `Vraćeno`, `Premješteno`, `Stornirano`.

| Radnja | Što se događa |
|---|---|
| Zaduživanje | oprema prelazi u `Zaduženo`, otvara se aktivno zaduženje |
| Povrat | zaduženje prelazi u `Vraćeno`, oprema se vraća na `Na skladištu` |
| Prijenos | staro zaduženje se zatvara kao `Premješteno`, otvara se novo s vezom `PreviousAssignmentId` na prethodno |
| Storniranje | poništava pogrešno unesen zapis, bez brisanja |

Poslovna pravila koja API provodi: ista oprema ne može imati dva aktivna zaduženja istovremeno, otpisana oprema se ne može zadužiti, a datum povrata ne može biti raniji od datuma zaduženja. Zaduženja se ne brišu - povijest ostaje cjelovita.

### Inventure

Inventura se otvara za jednu lokaciju i dobiva popis stavaka - svu opremu koja bi po evidenciji trebala biti tamo. Statusi: `Nacrt`, `Otvorena`, `U tijeku`, `Završena`, `Zaključana`.

Svaka stavka bilježi očekivano stanje (lokacija, zaduženi zaposlenik) i stvarno stanje koje popisivač upisuje: je li pronađena, je li oštećena, na kojoj je lokaciji zatečena ako nije na svojoj, bilješku te tko je i kada provjerio. Iz toga se računa sažetak: ukupno, popisano, manjak, oštećeno, krivo mjesto.

Zaključana inventura je zatvoren dokument - pokušaj izmjene stavke ili statusa vraća `400`. Nositelj uloge `LocationResponsible` vidi i vodi samo inventure svoje lokacije; provjera se radi usporedbom `Inventory.LocationId` s claimom iz tokena, a ne skrivanjem gumba.

### Zahtjevi za opremom

Zaposlenik traži opremu preko `/my-requests`, bez pristupa ostatku sustava. Zahtjev prolazi kroz statuse `Zaprimljeno` → `U obradi` → `Odobreno` ili `Odbijeno` → `Realizirano` → `Zatvoreno`.

Obrada je posao uloge `InventoryManager`. Realizacija zahtjeva povezuje odobreni zahtjev s konkretnom opremom i odmah stvara zaduženje, pa se ne mora raditi u dva koraka.

### Otpis

Otpis je odvojen postupak jer ima posljedicu koja se ne vraća. Statusi: `Zaprimljeno`, `U obradi`, `Odobreno`, `Odbijeno`, `Provedeno`.

Zahtjev za otpisom smije podnijeti `InventoryManager` ili `LocationResponsible`, obraditi ga `InventoryManager`, ali **provedbu smije potvrditi samo `Admin`** (`POST /api/writeoffrequests/{id}/execute`). Provedbom oprema prelazi u status `Otpisano` i od tog trenutka se više ne može zadužiti.

### Početna stranica

Brojači se računaju na API-ju (`GET /api/dashboard/summary`), s `CountAsync` nad upitima prema bazi - aplikacija ne dovlači zapise pa ih broji u pregledniku. Prikazuju se ukupna oprema i raspodjela po statusima, otvoreni zahtjevi, inventure u tijeku te posljednjih pet događaja po modulima. Zaposlenik na istoj stranici vidi svoje brojeve: koliko opreme ima zaduženo i koliko mu je zahtjeva u obradi.

### Šifrarnici i administracija

Lokacije i zaposlenici imaju vlastite CRUD ekrane. `InventoryManager` ih vidi, ali unos, izmjenu i brisanje smije samo `Admin` - to su podaci na koje se veže cijela evidencija. Vrste lokacija, kategorije opreme i svi statusi su šifrarnici koji se pune migracijom (`HasData`) i dolaze s API-ja - nijedna padajuća lista u sučelju nije tvrdo kodirana.

Administracija računa i uloga (`/users`) je dostupna samo ulozi `Admin`: stvaranje računa za postojećeg zaposlenika, dodjela uloga, aktivacija i deaktivacija te promjena lozinke.

### Ekrani

| Putanja | Ekran | U izborniku za |
|---|---|---|
| `/` | Početna s brojačima i zadnjim događajima | sve prijavljene |
| `/login` | Prijava | neprijavljene |
| `/equipment`, `/equipment/create`, `/equipment/{id}`, `/equipment/edit/{id}` | Oprema: popis, unos, profil, uređivanje | Admin, InventoryManager, LocationResponsible |
| `/assignments`, `/assignments/create` | Zaduženja | Admin, InventoryManager, LocationResponsible |
| `/inventories`, `/inventories/{id}` | Inventure i popis stavaka | Admin, InventoryManager, LocationResponsible |
| `/equipment-requests` | Obrada zahtjeva za opremom | Admin, InventoryManager |
| `/write-off-requests` | Otpis | Admin, InventoryManager |
| `/locations`, `/employees` (+ `/create`, `/edit/{id}`) | Šifrarnici | Admin, InventoryManager |
| `/my-equipment`, `/my-requests` | Vlastita oprema i vlastiti zahtjevi | sve prijavljene |
| `/users` | Korisnici i uloge | Admin |

Stupac govori komu se stavka pojavljuje u izborniku (`NavMenu.razor`, kroz `AuthorizeView`). To je stvar preglednosti, a ne zaštite: tko upiše putanju ručno, stranica će se otvoriti, ali će poziv API-ja vratiti `403` i korisnik završava na ekranu s porukom da nema ovlasti. Podatak ne izlazi iz API-ja ni u jednom slučaju.

Uz ekrane ide petnaestak MudBlazor dijaloga za radnje koje ne zaslužuju vlastitu stranicu - povrat, prijenos, storniranje, obrada zahtjeva, provjera stavke inventure, QR naljepnica i skener, AI sažetak, promjena lozinke.

## Pokretanje

Potreban je .NET 10 SDK.

1. Otvori `skafetin/skafetin.slnx` u Visual Studiju i postavi da se pokreću **oba** projekta, ili ih pokreni iz dva terminala:

   ```bash
   dotnet run --project skafetin/Skafetin.Api --launch-profile https
   dotnet run --project skafetin/Skafetin.App --launch-profile https
   ```

2. Postavi ključ za potpisivanje tokena (najmanje 32 znaka), u mapi `skafetin/Skafetin.Api`:

   ```bash
   dotnet user-secrets set "Jwt:SigningKey" "<kljuc-od-najmanje-32-znaka>"
   ```

   Bez njega se API neće pokrenuti; ključ namjerno ne stoji u repozitoriju.

3. Adrese:

   | Projekt | HTTPS | HTTP |
   |---|---|---|
   | `Skafetin.App` | `https://localhost:7205` | `http://localhost:5178` |
   | `Skafetin.Api` | `https://localhost:7126` | `http://localhost:5117` |

   Swagger je na `https://localhost:7126/swagger`. App čita adresu API-ja iz `ApiBaseUrl` u `Skafetin.App/appsettings.json`.

Baza se stvara sama: migracije se primjenjuju pri pokretanju API-ja, a zatim se puni demo sadržaj. Za potpuno svjež start obriši `skafetin/Skafetin.Api/Skafetin.db` i pokreni API ponovno.

## Demo računi

Svi računi imaju lozinku `Skafetin1!`. Riječ je o demo podacima; stvarne lozinke se ne spremaju u repozitorij, a u bazi stoje samo hash i sol.

| Korisničko ime | Uloge | Što vidi |
|---|---|---|
| `admin` | Admin | sve, uključujući administraciju računa i uloga |
| `marko.juric` | InventoryManager + LocationResponsible | **račun s dvije uloge** - svi moduli imovine, plus inventure svoje lokacije |
| `petra.kovacevic` | InventoryManager | oprema, zaduženja, inventure, zahtjevi, otpisi |
| `ana.peric` | LocationResponsible | oprema i inventure svoje lokacije |
| `josip.matic` | Employee | vlastita zadužena oprema i vlastiti zahtjevi |
| `nikolina.radic` | Employee | isto kao gore |

## Uloge i ovlasti

| Uloga | Što vidi | Što smije |
|---|---|---|
| `Admin` | sve | sve, uključujući šifrarnike i korisničke račune |
| `InventoryManager` | sve module imovine | unos i uređivanje opreme, zaduživanje, povrat, prijenos, inventure, obrada zahtjeva i otpisa |
| `LocationResponsible` | opremu i inventure **svoje** lokacije | provođenje inventure svoje lokacije |
| `Employee` | vlastitu zaduženu opremu i vlastite zahtjeve | slanje zahtjeva za opremom |

Politike su definirane u `Skafetin.Api/Security/AuthorizationPolicies.cs`:

- `AdminOnly` - samo `Admin`
- `Manage` - `Admin` i `InventoryManager`
- `InventoryWork` - `Admin`, `InventoryManager` i `LocationResponsible`

`FallbackPolicy` u `Program.cs` traži prijavljenog korisnika za **svaki** endpoint, pa je zaštita uključena i na rutama bez izričitog atributa.

## Računi i poslovni podaci

Prijava i poslovni podaci su odvojeni:

```
AppUser        ← podaci za prijavu (username, hash lozinke, sol)
AppUserRole    ← jedna ili više uloga po računu
AppRole        ← Admin, InventoryManager, LocationResponsible, Employee
Employee       ← poslovni profil: ime, e-pošta, radno mjesto, lokacija
```

`AppUser` je povezan sa `Employee`, a ne zamjenjuje ga. Zaduženja, zahtjevi i inventure vežu se na zaposlenika, dok račun nosi samo identitet i ovlasti. Id zaposlenika i lokacije putuju u JWT tokenu kao claimovi (`AppClaimTypes`), pa ih rute tipa `/mine` čitaju **iz tokena**, nikad iz zahtjeva.

Osobne rute: `GET /api/assignments/mine`, `GET /api/equipmentrequests/mine`, `POST /api/equipmentrequests/mine`.

## Pregled API-ja

Cjelovit popis s tijelima zahtjeva i odgovorima je u Swaggeru (`https://localhost:7126/swagger`). Ovdje je pregled skupina i tko im smije pristupiti.

| Skupina ruta | Sadržaj | Politika |
|---|---|---|
| `/api/auth` | prijava, podaci o prijavljenom korisniku | otvorena prijava, ostalo prijavljen korisnik |
| `/api/equipment` | CRUD, popis sa serverskim filtriranjem, `lookup`, premještanje, QR kod | čitanje prijavljen, izmjene `Manage`, brisanje `AdminOnly` |
| `/api/equipmentmedia` | slike i dokumenti uz opremu, naslovna slika | čitanje prijavljen, upload i brisanje `Manage` |
| `/api/assignments` | zaduženja, povrat, prijenos, storniranje, `mine` | čitanje `InventoryWork`, izmjene `Manage`, `mine` prijavljen korisnik |
| `/api/inventories` | inventure, stavke, promjena statusa | `InventoryWork`, uz dodatnu provjeru lokacije |
| `/api/equipmentrequests` | zahtjevi za opremom, obrada, realizacija, `mine` | obrada `Manage`, `mine` prijavljen korisnik |
| `/api/writeoffrequests` | zahtjevi za otpisom, obrada, provedba | unos `InventoryWork`, obrada `Manage`, provedba `AdminOnly` |
| `/api/locations`, `/api/employees` | lokacije i zaposlenici, puni CRUD i `lookup` rute | lokacije čitanje prijavljen, popis zaposlenika `Manage`, sve izmjene i brisanja `AdminOnly` |
| `/api/equipmentcategories`, `/api/locationtypes`, `/api/equipmentstatuses`, `/api/assignmentstatuses`, `/api/inventorystatuses`, `/api/requeststatuses`, `/api/writeoffrequeststatuses` | šifrarnici samo za čitanje | prijavljen korisnik |
| `/api/users`, `/api/roles` | računi i uloge | `AdminOnly` |
| `/api/dashboard/summary` | agregati za početnu stranicu | prijavljen korisnik |
| `/api/ai` | prijedlozi, opisani niže | vidi tablicu u odjeljku o AI-u |

## 401 i 403

| Status | Značenje | Kad se pojavi |
|---|---|---|
| `401 Unauthorized` | korisnik **nije prijavljen** ili je token istekao ili neispravan | poziv bilo kojeg endpointa bez `Authorization: Bearer` zaglavlja |
| `403 Forbidden` | korisnik **jest prijavljen**, ali nema pravo na taj podatak ili radnju | `Employee` zove endpoint pod politikom `Manage`; `LocationResponsible` otvara inventuru tuđe lokacije |

Zato kontroleri za tuđe podatke vraćaju `Forbid()`, a ne `Unauthorized()` - korisnik je poznat, samo mu podatak ne pripada. Skrivanje gumba u Blazoru je stvar iskustva, a ne zaštite; zaštitu provodi API.

## Datoteke uz opremu

Slike (`image/jpeg`, `image/png`, `image/webp`, najviše 5 MB) i PDF dokumenti (najviše 10 MB) spremaju se na disk, u mapu iz postavke `Storage:EquipmentMediaPath`, pod generiranim imenom. U bazi ostaju samo metapodaci: naziv, vrsta, izvorno ime datoteke i veza na opremu.

## AI prijedlozi

Aplikacija nudi dvije vrste prijedloga: **slobodan tekst** (sažetak inventure, obrazloženje zahtjeva) i **strukturirani odgovor** (popunjavanje obrasca nove opreme iz bilješke, provjera podataka prije spremanja). Prijedlog se korisniku uvijek prikazuje na pregled i **nikad se ne sprema sam** - korisnik ga izričito umeće, uređuje ili odbacuje, a kod unosa opreme popunjavaju se samo prazna polja.

### Endpointi i podaci koji se šalju provideru

| Endpoint | Politika | Što se šalje provideru |
|---|---|---|
| `GET /api/ai/status` | prijavljen korisnik | ništa; vraća naziv aktivnog providera, model i podatak koristi li se vanjska usluga |
| `POST /api/ai/inventory-summary/{inventoryId}` | `Manage` | kod inventure, naziv lokacije, je li zaključana i pet brojeva iz sažetka: ukupno, popisano, manjak, oštećeno, krivo mjesto |
| `POST /api/ai/request-draft` | prijavljen korisnik | naslov zahtjeva, naziv kategorije opreme i naziv zamjenske opreme ako je odabrana |
| `POST /api/ai/equipment-intake` | `Manage` | bilješka koju je korisnik sam upisao (najviše 4000 znakova) te nazivi kategorija i lokacija, kako bi se prepoznali |
| `POST /api/ai/equipment-check` | `Manage` | polja obrasca nove opreme i dvije oznake izračunate na API-ju: postoji li već taj inventurni i serijski broj |

Provideru se **ne šalju** osobni podaci zaposlenika, korisnički računi, lozinke ni hashevi, inventurni brojevi pojedinačne opreme ni nabavne vrijednosti. API sam dohvaća podatke iz baze i sastavlja kontekst; `IAiService` ne dobiva pristup `DbContext`-u, pa implementacija ne može proširiti opseg podataka koji izlaze iz aplikacije.

### Mock i vanjski provider

Zadani način rada je `Mock` - prijedlozi nastaju lokalno, bez ključa i bez internetske veze. To je stanje u `Skafetin.Api/appsettings.json`:

```json
"Ai": {
  "Provider": "Mock",
  "Model": "",
  "ApiKey": ""
}
```

Ključ se **nikad ne upisuje u `appsettings.json` ni u repozitorij**. Vanjski provider se uključuje kroz user secrets, u mapi `Skafetin.Api`:

```bash
dotnet user-secrets set "Ai:Provider" "OpenAI"
dotnet user-secrets set "Ai:Model" "<naziv modela>"
dotnet user-secrets set "Ai:ApiKey" "<kljuc>"
```

Povratak na lokalni način:

```bash
dotnet user-secrets set "Ai:Provider" "Mock"
```

Nakon ponovnog pokretanja API-ja kartica na početnoj stranici prikazuje aktivni provider. Ako je postavljen provider za koji ne postoji implementacija, ili nedostaje ključ ili naziv modela, aplikacija se vraća na `Mock` i zapisuje upozorenje u log; kartica tada prikazuje `Mock`, jer prikazuje ono što se stvarno izvršava, a ne ono što piše u konfiguraciji.

Strukturirani odgovori vanjskog providera traže se kroz JSON shemu, a odgovor se nakon dolaska **ponovno provjerava u aplikaciji**: identifikatori kategorije i lokacije moraju postojati u poslanim popisima, negativna vrijednost se odbacuje, duljine polja se skraćuju na dopuštene. Shema jamči oblik odgovora, ne njegovu istinitost.

## QR kod opreme

`GET /api/equipment/{id}/qr` vraća PNG s kodom koji vodi na profil te opreme. Naljepnica se ispisuje s profila opreme, a skener u aplikaciji (kamera preglednika) koristi se na tri mjesta: pronalazak opreme s popisa, označavanje stavke u otvorenoj inventuri i odabir opreme pri zaduživanju.

Adresa koja se upisuje u kod dolazi iz postavke `AppBaseUrl` u `Skafetin.Api/appsettings.json`; dok je ona `localhost`, naljepnica se ne može skenirati s drugog uređaja. Kamera u pregledniku radi samo preko HTTPS-a ili na `localhost`.

## Provjereni tokovi

Provjereno ručno kroz sučelje, na bazi stvorenoj iz nule: obrisan `Skafetin.db`, pokretanjem API projekta primijenjene migracije, šifrarnici i demo sadržaj.

| Tok | Rezultat |
|---|---|
| Pokretanje iz prazne baze - migracije, šifrarnici, seed podaci i demo računi | prolazi |
| Swagger se otvara, poziv bez tokena vraća `401` | prolazi |
| Prijava i odjava za sve četiri uloge | prolazi |
| Račun s dvije uloge (`marko.juric`) vidi zbroj ovlasti obiju: na popisu inventura sve lokacije, dok `ana.peric` vidi samo svoju | prolazi |
| Unos opreme → zaduženje → prijenos → povrat; povijest sadrži sve zapise | prolazi |
| Otvaranje inventure → popis stavaka → završetak → zaključavanje → pokušaj izmjene vraća `400` | prolazi |
| Zahtjev zaposlenika → obrada → odobrenje → realizacija | prolazi |
| Zahtjev za otpisom → odobrenje → provedba → pokušaj zaduženja otpisane opreme vraća `400` | prolazi |
| Pretraga, filtri i reset na ekranima Oprema i Inventure | prolazi |
| Loading i error stanja (ugašen API pa otvorena stranica) | prolazi |
| Upload: `.exe` odbijen, prevelik PDF odbijen, slika prolazi i prikazuje se | prolazi |
| Brisanje dokumenta uklanja i datoteku s diska | prolazi |
| Brojači na početnoj stranici odgovaraju stanju u bazi | prolazi |
| U bazi nema lozinke u čistom tekstu, spremaju se samo hash i sol | prolazi |


## Poznata ograničenja

- Baza je SQLite u datoteci uz API projekt, pa aplikacija podnosi samo jednu instancu.
- Datoteke uz opremu spremaju se na lokalni disk, bez vanjske pohrane.
- `MockAiService` je generator teksta po predlošku nad podacima iz baze - nije jezični model i ne zaključuje. `OpenAiService` šalje podatke iz gornje tablice vanjskoj usluzi. Zadano je uključen mock, pa aplikacija radi bez ključa, internetske veze i troška.
- Skener QR koda traži HTTPS i kameru; na uređaju bez kamere gumb javlja da skeniranje nije moguće.
- Aplikacija nije objavljena na javnoj adresi; pokreće se lokalno.

## Dokumentacija

- `docs/skafetin.dbml` - dijagram baze, svih 20 tablica s vezama. Otvara se na https://dbdiagram.io.
- Swagger na `https://localhost:7126/swagger` - cjelovit ugovor API-ja s tijelima zahtjeva i odgovorima.