# sterowanie-pojazdem
FT Optix – Faceplate Demo (symulator + PID, popupy, multi‑language)
## Opis projektu
Demonstracyjny projekt HMI w FactoryTalk Optix pokazujący:
- Sterowanie „pojazdami” (Car, Tractor, Motor) poprzez faceplate’y dziedziczące po wspólnym typie.
- Symulację prędkości z regulacją PID sterowaną przyciskami (gaz/hamulec/silnik).
- Popupy alarmowe wyzwalane zmianą zmiennych/tagów.
- Wielojęzyczność – zmiana języka jednym przyciskiem.

Projekt nie wymaga PLC – cała logika działa lokalnie w NetLogic Runtime (C#).
<img width="1601" height="984" alt="image" src="https://github.com/user-attachments/assets/282b5ace-9c07-4417-b0a4-01e4f6278fb2" />
## Najważniejsze funkcje

### Faceplate Template:
Wspólny typ dla jednostek napędowych (pojazdów), ułatwia skalowanie i re‑użycie.

### NetLogic – SpeedHandler:
PID (Kp/Ki/Kd) sterujący prędkością Speed na podstawie stanu: Engine, Accelerator, Brake.
Pętla sterowania z interwałem updateIntervalMs.
Inercja (powolne wytracanie prędkości) i szybkie hamowanie przy wyłączonym silniku.

### NetLogic – DialogTrigger:
Subskrypcje trzech wejść (InputVariable1..3) i otwieranie odpowiednich popupów (AlarmPopup1..3).
Obsługa zdalnych tagów przez RemoteVariableSynchronizer (jeśli kiedyś podłączysz PLC).


### Wielojęzyczność
Przełączanie języka poprzez przycisk UI.
