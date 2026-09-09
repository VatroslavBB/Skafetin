# Škafetin - provjera zahtjeva

Popis po kojem se projekt ocjenjuje. Stanje: `[ ]` nije, `[~]` u tijeku ili nepotvrđeno, `[x]` gotovo.

Kriteriji označeni s `[x]` provjereni su u kodu, uz navedeno mjesto. Stavke koje traže pokretanje aplikacije stoje na `[~]` ili `[ ]` dok se ne odrade.

## Obavezni tehnički kriteriji

| # | Kriterij | Stanje | Gdje je ispunjeno |
|---|---|---|---|
| 1 | Potpuni CRUD: GET lista, GET po Id, POST, PUT, opravdan DELETE | [x] | `LocationsController`, `EquipmentController`, `EmployeesController`; `DELETE` postoji na lokacijama, opremi, zaposlenicima i datotekama, a namjerno ga nema na zaduženjima, inventurama i otpisima |
| 2 | Relevantni HTTP odgovori: 200, 201, 204, 400, 401, 403, 404 | [x] | `CreatedAtAction` u devet kontrolera, `NoContent` na izmjenama i brisanjima, `BadRequest` s `ErrorResponseDto` na poslovnim pravilima, `NotFound` na nepostojećem id-u; 401 iz `FallbackPolicy`, 403 iz politika i provjere lokacije |
| 3 | Najmanje dva tablična prikaza s pretragom, dva filtera, sortiranjem i resetom | [x] | `Equipment.razor` (kategorija, status, lokacija, zaposlenik), `Inventories.razor`, `Locations.razor`; sve tri imaju gumb za čišćenje filtara |
| 4 | Najmanje jedan popis sa serverskim filtriranjem | [x] | `EquipmentController.GetEquipment` i `InventoriesController.GetItems` - pretraga, filtri, sortiranje i stranicanje idu kroz upit nad bazom, ne u pregledniku |
| 5 | Lookup vrijednosti dolaze iz API-ja | [x] | sedam kontrolera šifrarnika plus `lookup` rute na opremi, lokacijama, zaposlenicima i ulogama; nijedna padajuća lista nije tvrdo kodirana |
| 6 | Dashboard agregate računa API iz baze | [x] | `DashboardController.GetSummary` - brojači se računaju s `CountAsync` nad upitima, App samo prikazuje rezultat |
| 7 | Upload s validacijom tipa i veličine, sigurnim imenom i metapodacima | [x] | `EquipmentMediaController.Upload` - dopušteni su `image/jpeg`, `image/png`, `image/webp` do 5 MB i `application/pdf` do 10 MB; ime datoteke na disku je `Guid`, u bazi ostaju metapodaci |
| 8 | Najmanje jedan račun s više uloga | [x] | `AppUserSeeder` - račun `marko.juric` ima `InventoryManager` i `LocationResponsible` |
| 9 | Autorizacija provedena na API strani | [x] | `FallbackPolicy` traži prijavljenog korisnika za svaki endpoint; politike `AdminOnly`, `Manage` i `InventoryWork` u `AuthorizationPolicies.cs` |
| 10 | Najmanje jedan `/mine` endpoint s identitetom iz JWT claima | [x] | `AssignmentsController` `GET mine`, `EquipmentRequestsController` `GET mine` i `POST mine`; id zaposlenika se čita iz claima, nikad iz zahtjeva |
| 11 | Rješenje se builda bez grešaka | [~] | zadnji `dotnet build` prošao je bez grešaka; ponoviti nakon zadnjih izmjena |
| 12 | README s uputama za pokretanje i demo računima | [x] | `README.md` - pokretanje, user secrets, adrese, demo računi, uloge, 401/403 |

## Poslovna pravila iz specifikacije

| # | Pravilo | Stanje | Gdje |
|---|---|---|---|
| 1 | Inventurni broj obavezan i jedinstven | [x] | jedinstveni indeks u `SkafetinDbContext` (`HasIndex(e => e.InventoryNumber).IsUnique()`) i provjera u `EquipmentController` |
| 3 | Nema dva aktivna zaduženja iste opreme | [x] | `AssignmentsController.CreateAssignment` - "Odabrana oprema već ima aktivno zaduženje." |
| 4 | Prijenos zatvara staro kao Premješteno i otvara novo | [x] | `AssignmentsController.TransferAssignment`, veza kroz `PreviousAssignmentId` |
| 5 | Povrat ne prije zaduženja | [x] | `AssignmentsController.ReturnAssignment` uspoređuje `AssignedAt` i `ReturnedAt`; ista provjera i u dijalogu |
| 8 | Otpisana oprema se ne može zadužiti | [x] | provjera `EquipmentStatusWriteOff` pri zaduženju i prijenosu |
| 9 | Zaposlenik vidi samo svoje | [x] | `/mine` rute i claim `employeeId`; stranice `MyEquipment` i `MyRequests` |
| 10 | Odgovorna osoba mijenja samo svoju lokaciju | [x] | `InventoriesController.LoadInventoryAsync` usporedbom `Inventory.LocationId` s claimom, uz `Forbid()` |
| 11 | Zaključana inventura se ne mijenja | [x] | provjera `InventoryStatusLocked` pri izmjeni stavke i promjeni statusa |
| 13 | Povijest se ne briše | [x] | nema `DELETE` ruta na zaduženjima, inventurama ni otpisima; `EquipmentStatusHistory` se samo dopunjava |
| 15 | Lozinke hashirane | [x] | `PasswordHasher` sprema hash i sol; u bazi nema lozinke u čistom tekstu |

## Uvjeti predaje

| Uvjet | Stanje | Gdje |
|---|---|---|
| Izvorni kod s migracijama | [x] | `Skafetin.Api/Migrations` - `AddSkafetinSchema` i `MissingSchemaChanges` |
| Seed podaci za sve module | [x] | `Data/SeedData.cs`, `SeedData.History.cs`, `SeedData.Media.cs`, `SeedData.Requests.cs`, uz `HasData` za šifrarnike i uloge |
| README: pokretanje, demo računi, uloge | [x] | `README.md` |
| Demo korisnici za sve četiri uloge | [x] | `AppUserSeeder` - šest računa, sve četiri uloge, jedan račun s dvije |
| DBML dijagram | [x] | `docs/skafetin.dbml`, kopija u repozitoriju `Skafetin/docs/skafetin.dbml` - svih 20 tablica i sva polja provjerena protiv modela i `SkafetinDbContext`-a |
| Popis provjerenih workflowa | [x] | `README.md`, odjeljak "Provjereni tokovi" |
| Pokretanje iz prazne baze | [x] | obrisan `Skafetin.db`, pokretanje Api projekta stvara bazu i seed |

## Uvjetni dio (bonusi i objava)

| Stavka | Stanje | Gdje |
|---|---|---|
| QR kod opreme - naljepnica i skener | [x] | `GET /api/equipment/{id}/qr`, `QrScannerDialog` i `QrCodeReader`; skener na popisu opreme, u inventuri i pri zaduživanju |
| AI prijedlozi (`IAiService`, `MockAiService`, `OpenAiService`) | [~] | `/api/ai/*`; napisano i prevedeno, provjera kroz sučelje nije dovršena |
| Vremenska crta opreme s dokumentima | - | izvan opsega, odlukom uz B1 u `04-plan-izrade.md` |
| Analiza odstupanja i vrijednosti po lokacijama | - | izvan opsega, odlukom uz B2 u `04-plan-izrade.md` |
| Objava demo verzije na javnoj adresi | [ ] | faza 14 nije rađena; projekt se pokreće lokalno |

## Provjere prije predaje

Odraditi redom, na svježe kloniranom repozitoriju:

- [ ] `dotnet build` prolazi bez grešaka
- [x] obrisana `Skafetin.db`, pokretanje Api projekta stvara bazu i seed
- [ ] Swagger se otvara, poziv bez tokena vraća 401
- [ ] prijava i odjava rade za sve četiri uloge
- [ ] račun s dvije uloge vidi zbroj ovlasti obiju
- [ ] tok: unos opreme → zaduženje → prijenos → povrat, povijest ima sve zapise
- [ ] tok: otvaranje inventure → popis stavaka → završetak → zaključavanje → pokušaj izmjene vraća 400
- [ ] tok: zahtjev zaposlenika → obrada → odobrenje → realizacija
- [ ] tok: zahtjev za otpisom → odobrenje → provedba → pokušaj zaduženja vraća 400
- [ ] pretraga i filtri rade na `Equipment` i `Inventories`, reset vraća sve
- [ ] loading i error stanja se vide (ugasiti Api pa otvoriti stranicu)
- [ ] upload: `.exe` odbijen, prevelik PDF odbijen, slika prolazi i vidi se
- [ ] brisanje dokumenta uklanja i datoteku s diska
- [ ] dashboard brojači odgovaraju stvarnom stanju u bazi
- [ ] u bazi nema nijedne lozinke u čistom tekstu
- [ ] `appsettings.json` u gitu nema pravi JWT ključ ni AI ključ
