# Škafetin - e-Imovina Županije

Aplikacija za evidenciju imovine županije: popis opreme po lokacijama i kategorijama, zaduživanje opreme zaposlenicima s cjelovitom poviješću, inventure po lokacijama sa stavkama i odstupanjima, zahtjevi zaposlenika za opremom, postupak otpisa te slike i dokumenti uz opremu. Pristup je moguć tek nakon prijave, a što korisnik vidi i smije ovisi o njegovim ulogama.

## Tehnologije

.NET 10, ASP.NET Core Web API, Blazor Server s MudBlazor, EF Core i SQLite, JWT autentifikacija.

| Projekt | Uloga |
|---|---|
| `Skafetin.Api` | Web API, baza, autentifikacija i autorizacija, Swagger |
| `Skafetin.App` | Blazor Server korisničko sučelje |
| `Skafetin.Shared` | Modeli i DTO klase koje koriste oba projekta |

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

_Popuniti nakon provjere iz prazne baze._

## Poznata ograničenja

- Baza je SQLite u datoteci uz API projekt, pa aplikacija podnosi samo jednu instancu.
- Datoteke uz opremu spremaju se na lokalni disk, bez vanjske pohrane.
- `MockAiService` je generator teksta po predlošku nad podacima iz baze - nije jezični model i ne zaključuje. `OpenAiService` šalje podatke iz gornje tablice vanjskoj usluzi. Zadano je uključen mock, pa aplikacija radi bez ključa, internetske veze i troška.
- Skener QR koda traži HTTPS i kameru; na uređaju bez kamere gumb javlja da skeniranje nije moguće.
- Aplikacija nije objavljena na javnoj adresi; pokreće se lokalno.

## Dokumentacija

Opseg, model podataka, ugovor API-ja, plan izrade, provjera zahtjeva i git workflow su u mapi `docs/` izvan ovog repozitorija, uz DBML dijagram baze (`docs/skafetin.dbml`).
