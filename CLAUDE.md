# CLAUDE.md – Penguin After All (OlcSideScrollingConsoleGame)

Detta dokument läses automatiskt av Claude i början av varje session.
Det styr hur refaktoreringen av det här projektet ska bedrivas.

---

## Vad det här projektet är

Ett C# side-scrolling platformer-spel med en pingvin som protagonist.
Byggt som ett tinkerprojekt utan ursprunglig arkitekturambition.
Målet med pågående arbete är att modernisera och strukturera koden
enligt SOLID-principerna — **utan att förändra spelupplevelsen**.

**Detaljerad planering och historik:** Se `../REFACTORING_PLAN.md`
(en nivå upp från repo-roten, dvs `C:\AI\Claude\REFACTORING_PLAN.md`)

---

## Teknisk miljö

- **Språk:** C# / .NET Framework 4.7.2
- **Spelmotor:** DevChrome Pixel-Engine (C#-port av olcPixelGameEngine)
- **Testramverk:** MSTest (projektet `UnitTest/`)
- **Byggsystem:** MSBuild / Visual Studio
- **Bibliotek (egna, källkod tillgänglig):**
  - `Audio.Library` — OpenAL-ljud via OpenTK (`../AudioConsole/`)
  - `Gamepad.Library` — Xbox-gamepad via SlimDX (`../Gamepad/`)
  - `PixelEngine` — spelmotor (`../PixelEngineNet/`)

---

## Kodregel 1 — Läsbarhet är viktigare än smarthet

> "Kod skrivs en gång men läses hundra gånger."

När två tillvägagångssätt är tekniskt likvärdiga **ska det mer lättlästa alltid väljas**.

Det innebär i praktiken:
- Tydliga, beskrivande namn på klasser, metoder och variabler (på engelska)
- Korta metoder med ett enda ansvarsområde
- Kommentarer på svenska är okej — koden är på engelska
- Undvik smarta one-liners om en tydligare form finns
- Undvik djup nästling — bryt ut till metod istället
- Magiska tal är förbjudna — använd `GameConstants`

---

## Kodregel 2 — SOLID är ledstjärnan

Varje förändring ska motiveras av minst en SOLID-princip:

| Princip | Kortversion | Vanligast i det här projektet |
|---------|-------------|-------------------------------|
| **SRP** | En klass, ett ansvar | `Program.cs` är 5 000+ rader — bryt ut system |
| **OCP** | Öppen för utökning, stängd för ändring | Ny fiende ska inte kräva ändring i `DrawSelf()` |
| **LSP** | Subklasser ska uppfylla basklassens kontrakt | Virtuella no-ops ska bli abstrakta metoder |
| **ISP** | Smala interface hellre än breda | Dela upp stora interface |
| **DIP** | Beroende på abstraktioner, inte konkretioner | `IInputProvider` är mallen |

---

## Kodregel 3 — Projektet ska alltid gå att bygga

Refaktorering sker steg för steg. **Varje commit ska kompilera.**
Testa bygget lokalt innan commit:

```
msbuild PenguinAfterAll.sln /p:Configuration=Debug
```

eller via Visual Studio. Bryt aldrig bygget och lämna det trasigt.

---

## Kodregel 4 — Ett led i taget

Slutför ett refaktoreringssteg fullständigt — inklusive enhetstester —
innan nästa påbörjas. Halv-extraherade klasser är värre än ingen extrahering.

---

## Kodregel 5 — Tester för all ny logik

Ny kod som innehåller logik ska ha enhetstester i `UnitTest/`-projektet.
Tester ska köras utan hårdvara (ingen gamepad, inget fönster).
Använd `FakeInputProvider` som mall för test-fakes.

---

## Pågående refaktorering — nuläge

### Klart ✅
- `GameConstants.cs` — alla magic numbers samlade
- `IInputProvider` + `InputManager` — input abstraherad (DIP)
- `FakeInputProvider` — testfake för hårdvarufri testning
- `PhysicsSystem.cs` — gravitation, luftmotstånd, hastighetsbegränsning (SRP)
- 24 MSTest-tester (GameConstantsTests, InputProviderTests, PhysicsSystemTests)
- Bygget är grönt ✅

### Nästa steg (i prioritetsordning)
1. `CameraSystem` — kameraberäkning ur `DisplayStage`
2. `RenderSystem` — tile- och sprite-ritning ur `DisplayStage`
3. Namnbaserad dispatch → polymorfism (`if Name == "boss"` etc.)
4. Singleton-beroenden → dependency injection

---

## Vad som INTE ska göras utan explicit diskussion

- Byta spelmotor (PixelEngine → MonoGame etc.) — separat beslut
- Ändra spelmekanik eller spelupplevelse
- Bryta mot .NET Framework 4.7.2-kompatibilitet
- Stora refaktoreringar i ett enda commit — dela upp dem
