# OpenWorldGame — GTA 6-like открытый мир на Unity 6000

**Мега-апдейт:** полиция с погонями, день/ночь, сохранения, магазины, улучшенная физика, 4 пушки + гранаты, квартиры с доходом, 8 способов фарма — всё в одном городе 1км.

## Мир 1км × 1км

- **Город 998м** — `CityGenerator` 12×12 кварталов (68м + дороги 14м). Здания с **256×256 оконными текстурами** (`MaterialLibrary` — Perlin 8×8 сетка, рандом подсветки), асфальт с шумом, белая разметка каждые 6м.
- **Горы** — кольцо коробок 60-155м + сферы-пики, `isStatic` батчинг, не даёт выпасть.
- **Небо** — `Skybox/Procedural` (`_SkyTint 0.5/0.5/0.6`, `_GroundColor`, `_Atmosphere 1.05`, `_Exposure 1.25`), `RenderSettings.ambientMode Skybox`, туман `0.0022`, даль `2000м`.
- **День/Ночь** — `DayNightCycle` 90с цикл, вращает солнце `360°`, меняет `intensity 0.12→1.18`, `ambientIntensity 0.35→1.05`, `fogDensity` — ночь/день.

## Физика как в GTA

`CarController` — аркада: `CoM -0.9`, `motor 1850`, `steer 34°` с падением `0.42×` на 80км/ч, `brake 5000`, `grip 1.15→0.52` на ручнике, `downForce 65`, дрифт `AddForce(-lat*0.55)` + дым шин `Effects.TireSmoke` + крен `2.8°`. Колеса `WheelCollider 35k/4.5k`.

`CarDamage` — 100 HP, `OnCollisionEnter` урон `*1.8`, потемнение, вмятины `scale`, дым `<40%`, взрыв `AddForce 4200`, падение `torque 650-1350`, `Repair()`.

**Угон:** `CarInteraction` видит `TrafficCar` → `E — угнать` → конвертирует в `CarFactory.Create` + `CarDamage`.

## Экономика — 8 способов

| Способ | Файл | $ |
|---|---|---|
| Такси | `JobManager Taxi` | 120-200 |
| Курьер | `Courier` | 90-150 |
| Сбор | `Collect` | 60-100 |
| Монетки 36 | `MoneyPickup` | 12-40 |
| Банкомат 5 | `ATM` | 250/45с |
| Скупка  | `ChopShop` 10×12м | 300-600 по целосности |
| Гонка 5 чекпоинтов | `RaceManager` | 750+300 |
| Квартиры пассив | `Apartment` | 35-140/мин |

`PlayerWallet` — синглтон, `PlayerPrefs` + `SaveManager` JSON.

## Квартиры 3

`Apartment` 6×5м с интерьером `+12м` (пол 8м) на блоках 4,4 / 8,8 / 5,9: `$1800/35`, `$3500/75`, `$6200/140`. `E` купить/войти, `H` продать 50%, `incomeTimer 60с`. Сохраняются `PlayerPrefs "Apt_x_z"`.

## Полиция и розыск ★

`WantedSystem` — `Stars 0-5`, `AddStar()` на выстрел/убийство педа/гранату, `decay 25с` если нет копов в `42м`, спавн `PoliceCar.Create` (белый+синяя полоса+мигалка красно-синяя) `1-3` шт, погоня `13м/с` `turn 140°`, таран `900` + урон педу `8`. `Despawn` при 0 звезд.

## Магазины

- **Оружие** `WeaponShop` 5×3м — `1 Pistol $500 18патр, 2 Shotgun $1200 8патр, 3 Rifle $2500 30патр`
- **Автосалон** `CarShop` 7×3м — седан `$1800` спорт `$4500` внедорожник `$7000` → `CarFactory.Create`

## Оружие 4 + гранаты

База `Weapon` → `Pistol 35/120м/0.22с/18`, `Shotgun 8×18/45м/0.68с`, `Rifle 22/180м/0.10с авто`, `Grenade 9м/90dmg/OverlapSphere`. `WeaponInventory` на игроке — `1-4/Scroll` выбор, `G` бросок гранаты `Rigidbody 620+180`, `MuzzleFlash` сфера `0.06с`, `Explosion` сфера `0.42с`. Стрельба только пешком.

## Сущности

`Entity` 100 HP → `Pedestrian` 28 шт бродят по тротуарам `1.6м/с` `BlockCenter ± (blockSize-6)`, ждут `0.5-2с`, дроп `$30` + звезда.

## Управление

```
WASD ход/руль, Shift бег, Space прыжок/ручник
E — сесть/угнать, работа, банкомат, квартира, скупка, гонка
ЛКМ огонь, R перезарядка, G граната, 1-4 смена, H подсказки, Esc курсор
```

## UI

Мини-карта 220 `RenderTexture 256 ortho 170м` справа сверху, деньги по центру, звезды `★☆` по центру `42px`, задание `описание $ dist`, патроны `Pistol 12/18 [1-4/G]` справа снизу, скорость в тачке.

## Сохранения

`SaveManager` JSON `PlayerPrefs "OpenWorld_Save_v2"` — деньги, патроны, `GameManager` автосейв каждые `15с` + `PlayerWallet` загрузка при старте.

## Старт

1. Unity Hub → Unity **6000.5+** → https://unity.com/download
2. Add project from disk → `openworldgame` (не `My project/`)
3. Дождись компиляции — сцена `Assets/Scenes/Main.unity` соберётся сама, иначе `OpenWorld → Собрать демо-сцену` → Play

Совместимо с `2022.3 LTS` и `6000+`.

## Структура

```
Assets/Scripts/
├── City/CityGenerator.cs + Visuals/{MaterialLibrary,Effects}.cs
├── Vehicle/{CarController(GTA),CarDamage,CarFactory,TrafficCar/Spawner}
├── Police/{WantedSystem,PoliceCar}
├── World/{DayNightCycle,Apartment,ChopShop,RaceManager,ATM}
├── Economy/{PlayerWallet,MoneyPickup} + Save/SaveManager.cs
├── Jobs/{JobManager,JobMarker,JobGiver} + Entities/{Entity,Pedestrian}
├── Weapon/{Weapon,Pistol,Shotgun,Rifle,Grenade,WeaponInventory}
└── Editor/SceneBuilder.cs
```

## GitHub

https://github.com/olexandrmykhailovskyi-oss/openworldgame — `master` (38 файлов, Roslyn 0 ошибок).
