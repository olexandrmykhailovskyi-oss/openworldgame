# OpenWorldGame — GTA 6-like открытый мир на Unity 6000

Мега-апдейт: огромный город 1км, горы, трафик, экономика, работы, угон, квартиры, урон машин, GTA-физика, 4 вида оружия и гранаты.

## Мир 1км × 1км

- **Город 998м** — `CityGenerator` 12×12 кварталов (68м + дороги 14м). Здания с **оконными текстурами 256×256** (`MaterialLibrary` — процедурная генерация окон 8×8 с подсветкой, Perlin-шум, рамы), асфальт с шумом `GetAsphalt()`, тротуары штукатурка, белая разметка штрихами каждые 6м. ~1400 зданий.
- **Горы** — кольцо коробок 60-155м + сферы-пики, закрывает горизонт, `isStatic` для батчинга.
- **Небо** — `Skybox/Procedural` (`_SkyTint 0.5/0.5/0.6`, `_GroundColor`, `_AtmosphereThickness 1.05`, `_Exposure 1.25`), `RenderSettings.ambientMode Skybox`, туман `Exponential 0.0022`, даль камеры `2000м`.

## Машины — физика как в GTA

`CarController` — перенастроено под аркаду:
- Центр массы `-0.9,0.3`, `drag 0.08`, `motor 1850`, `steer 34°` с падением на скорости `0.42×`, `brake 5000`
- `sidewaysStiffness 1.15` → `0.52` на ручнике, `forward 1.05/0.75`, дрифт через `AddForce(-lat*0.55)`, прижим `65×velocity`, крен на поворотах.

`CarDamage` — здоровье 100, царапины (темнение цвета), вмятины (`localScale`), дым-куб при `<40%`, взрыв с `AddForce`, падение мощности `650-1350` и скорости `55-155 км/ч`. Чинится `Repair()`.

**Угон:** `CarInteraction` теперь видит `TrafficCar` (`FindNearestTraffic`) — `E — угнать` → конвертирует трафик в управляемый `CarFactory.Create` + `CarDamage`.

## Экономика — 7 способов заработать

| Способ | Файл | Сколько |
|---|---|---|
| **Такси** | `JobManager Taxi` | $120-200 |
| **Курьер** | `Courier` | $90-150 |
| **Сбор** | `Collect` | $60-100 |
| **Монетки** | `MoneyPickup` 36 шт | $12-40 |
| **Банкомат** | `ATM` 5 шт | $250/45с |
| **Скупка угнанных** | `ChopShop` у юга карты | $300-600 (зависит от целосности) |
| **Гонка** | `RaceManager` 5 чекпоинтов | $750+300 бонус |
| **Квартиры (пассив)** | `Apartment` 3 шт | $35-140/мин |
| **Дроп с NPC** | `Pedestrian.Die` | $30 |

`PlayerWallet` — синглтон, `PlayerPrefs` сохранение, `OnMoneyChanged`.

## Квартиры

3 квартиры (`Apartment.cs`): `$1800/$35`, `$3500/$75`, `$6200/$140` в `SceneBuilder:CreateApartments`. Куб 6×5м с интерьером (пол 8м). `E` купить/войти (телепорт на `+12м` вверх), `H` продать за 50%. Пассивный доход `incomeTimer += delta; if >=60s AddMoney`.

`ChopShop` — красный куб 10×12м на краю, сдай угнанную тачку на `E`.

`RaceManager` — старт у `(22,22)`, генерирует 5 чекпоинтов-цилиндров розовых по дорогам, таймер, бонус `<45с`.

## Оружие — 4 вида + инвентарь

База `Weapon.cs` → `Pistol`, `Shotgun` (8 дробинок, разброс 6), `Rifle` (авто `0.10с`, 30 пат), `Grenade` (3с фитиль, `radius 9м, 90 dmg`, `OverlapSphere`, `AddForce 900`).

`WeaponInventory` на игроке — `1-4` переключение, `Scroll`, `G` бросок гранаты (сфера 0.5м, `Rigidbody 1.2кг, 620+180` вверх). `Pistol/Shotgun/Rifle` рейкаст из `Camera.main`.

Пешком: `ЛКМ` огонь, `R` перезарядка. В машине стрельба отключена (`PlayerController.ControlEnabled == false`).

## Сущности

`Entity` — 100 HP, `TakeDamage`. `Pedestrian` — 28 NPC (`SceneBuilder:CreatePedestrians`) бродят по тротуарам `walkSpeed 1.6`, `BlockCenter ± (blockSize-6)`, ждут `0.5-2с`.

## Управление

```
WASD — ходьба/руль, Shift бег, Space прыжок/ручник
E — сесть/угнать, работа, банкомат, квартира, скупка, гонка
ЛКМ — огонь (пистолет/дробовик/винтовка), R — перезарядка
G — граната, 1-4 — смена оружия, H — подсказки/продажа
Мышь — камера, Esc — курсор
```

## Текстуры по красоте

`MaterialLibrary`:
- Окна 256: рамы 2px, 8×8 сетка, двойные стёкла, `PerlinNoise` шум, случайные включенные окна `Sin(seed)`, блики.
- Асфальт 128: `PerlinNoise 0.08/0.25`, вкрапления, `Bilinear`
- Штукатурка 128: светлая `0.88+0.07*Perlin`

Здания — `GetWindowMaterial`, дороги — `GetAsphalt`, горы — каменные 5 оттенков.

## Быстрый старт

1. Unity Hub → **Unity 6000.5+** → https://unity.com/download
2. Add project from disk → `openworldgame` (не `My project/`)
3. Дождись компиляции — сцена `Assets/Scenes/Main.unity` соберётся сама (`SceneBuilder InitializeOnLoad`), иначе `OpenWorld → Собрать демо-сцену`
4. Play

Совместимо с `2022.3 LTS` и `6000+`.

## Структура

```
Assets/Scripts/
├── City/CityGenerator.cs + Visuals/MaterialLibrary.cs
├── Vehicle/CarController.cs (GTA), CarDamage.cs, CarFactory.cs, TrafficCar.cs
├── Economy/PlayerWallet.cs, MoneyPickup.cs
├── Jobs/JobManager.cs, JobMarker.cs, JobGiver.cs
├── World/Apartment.cs, ChopShop.cs, RaceManager.cs, ATM.cs
├── Entities/Entity.cs, Pedestrian.cs
├── Weapon/Weapon.cs, Pistol.cs, Shotgun.cs, Rifle.cs, Grenade.cs, WeaponInventory.cs
└── Editor/SceneBuilder.cs
```

## GitHub

https://github.com/olexandrmykhailovskyi-oss/openworldgame — ветка `master`.
Папка `My project/` — кэш шаблона, игнорируется.
