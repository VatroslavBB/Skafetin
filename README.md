# Skafetin

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

Nakon ponovnog pokretanja API-ja kartica na početnoj stranici prikazuje aktivni provider. Ako je postavljen provider za koji ne postoji implementacija, aplikacija se vraća na `Mock` i zapisuje upozorenje u log; kartica tada prikazuje `Mock`, jer prikazuje ono što se stvarno izvršava, a ne ono što piše u konfiguraciji.

Zamjenom providera ne mijenjaju se Blazor stranice, DTO modeli ni rute - samo konfiguracija i klasa iza `IAiService`.

### Poznata ograničenja

Trenutna implementacija je `MockAiService`, generator teksta po predlošku nad podacima iz baze. Nije jezični model i ne zaključuje; služi da aplikacija ima dovršen tok i granicu na kojoj bi vanjski provider stajao.
