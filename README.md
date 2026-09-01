# OpenWorldGame — GTA SA / GTA 5 style на Unity 6000

**Ультра-апдейт:** звёзды за беспредел, умные NPC боятся оружия, собачки и котики, текстуры 1в1 как в GTA SA/5, 8 способов фарма, физика GTA, 4 пушки + гранаты, квартиры, угон, гонки, полиция, день/ночь — 41 скрипт, 1км город.

## Мир 1км × 1км — по красоте

- **Город 998м** — `CityGenerator` 12×12 кварталов (68м + дороги 14м). 
- **Текстуры GTA SA/5** `MaterialLibrary`:
  - Окна `512×512` 8×8 сетка — рамы 3px, подоконники, `Perlin` грязь, рандом включенных окон `Sin(seed)`, отражения, AC-блоки на фасадах, `Bilinear`
  - Кирпич `256` — кладка 64×32, шов 3px, смещение рядов, `Brick1/2` + мортар
  - Асфальт `256` — два `Perlin 0.06/0.22`, трещины `>0.72`, решётка 64px
  - Штукатурка `256` — `0.87+Perlin`
  - Выбор по высоте: `<18м` → кирпич, `≥18м` → стекло-окна. Дороги `GetAsphalt()`, разметка белая штрих `3.5м/6м`.
- **Горы** — кольцо 60-155м + сферы-пики, `isStatic` батчинг.
- **Небо** — `Skybox/Procedural` (`_SkyTint 0.5/0.5/0.6`, `_GroundColor`, `_Atmosphere 1.05`, `_Exposure 1.25`), `ambientMode Skybox`, туман `0.0022`, даль `2000м`.
- **День/Ночь** `DayNightCycle` — 90с цикл, солнце `360°`, `intensity 0.12→1.18`, `ambient 0.35→1.05`, `fog`.

## Геймплей — имба

### Звёзды розыска ★ (как в GTA)
`WantedSystem` — `CrimeType {Brandishing, Assault, Theft, Vandalism, Murder, Explosion, MassMurder}`:
- Выстрел `Assault +1`, убийство `Murder +2`, взрыв `+2`, серийные убийства `3 за 30с → MassMurder +3`
- Эскалация `massMurderStreak`, 4 звезды → 4 копа, 5 звезд → 5 копов, `decay 25с` если нет копов в `42м` + педы не звонят
- Педы звонят: `Pedestrian IsCallingPolice` в радиусе `25м` блокирует сброс

### NPC с нормальным ИИ
`Pedestrian` — `State {Wander, Flee, Cower, CallPolice}`:
- `senseWeaponDist 14м` — если у игрока `WeaponInventory.Current` в радиусе → `Flee 4-7с`
- `senseGunshotDist 32м` — на `NotifyGunshot(pos)` от `Weapon.DoRaycast` → побег
- Украевые: бег от игрока `18м` с клампом к городу, `6м/с` поворот `9°`, 35% шанс `CallPolice 3.5с` → `ReportCrime Assault`
- `Cower`/`Wander` возврат, `Die` → `$30` + `ReportCrime Murder`

### Питомцы — собачки и котики
`Pets/Dog.cs` — капсула 0.5×0.85м + голова сфера + уши-кубы + хвост цилиндр, коричневый `0.55,0.38,0.22`. Следует за игроком `>2.2м` `5м/с`, бродит `1.8м/с`, `E` гав ♥
`Pets/Cat.cs` — маленькая `0.38×0.62м`, 3 окраса (рыжий/белый/чёрный), уши-кубы, хвост. Бродит `1.35м/с`, сидит 15% шанс `2-4с`, `E` мурр ♥
`PetSpawner` — `7 собак + 10 котов` по паркам (`RandomParkPosition`)

### Физика GTA
`CarController` — `CoM -0.9,0.3`, `motor 1850`, `steer 34°` падение `0.42×` на 80км/ч, `brake 5000`, `grip 1.15→0.52` на ручнике, `downForce 65`, дрифт `-lat*0.55` + дым `Effects.TireSmoke`, крен `2.8°`.
`CarDamage` — 100 HP, царапины/дым/вмятины/взрыв.

**Угон:** `CarInteraction` → `TrafficCar` `E — угнать` → `CarFactory.Create` + `CarDamage`.

### Экономика — 8 способов

Такси $120-200, Курьер $90-150, Сбор $60-100, Монетки 36 $12-40, Банкомат 5 $250/45с, Скупка `ChopShop` $300-600, Гонка 5 чекпоинтов $750+300, Квартиры пассив $35-140/мин + дроп педов $30. `PlayerWallet` + `SaveManager` JSON.

### Квартиры 3
`Apartment` 6×5м интерьер `+12м` на блоках 4,4/8,8/5,9: `$1800/35`, `$3500/75`, `$6200/140`. `E` купить/войти, `H` продать 50%, `incomeTimer 60с`. `PlayerPrefs "Apt_x_z"`.

### Оружие 4 + гранаты
`Weapon` база → `Pistol 35/120м/0.22с/18`, `Shotgun 8×18/45м/0.68с`, `Rifle 22/180м/0.10с авто/30`, `Grenade 9м/90dmg/OverlapSphere`. `WeaponInventory` `1-4/Scroll`, `G` бросок `Rigidbody 620+180`. `MuzzleFlash` сфера `0.06с`, `Explosion` `0.42с`, Wanted на каждый выстрел `+1`.

## Управление

```
WASD ход/руль, Shift бег, Space прыжок/ручник
E — сесть/угнать, работа, банкомат, квартира, скупка, гонка, погладить питомца
ЛКМ огонь, R перезарядка, G граната, 1-4 смена, H подсказки, Esc курсор
```

## UI

Мини-карта 220 (орто 170м), деньги по центру, звезды `★☆` по центру `42px`, задание `описание $ dist`, патроны `[1-4/G]` справа, скорость в тачке.

## Старт

1. Unity Hub → **Unity 6000.5+** → https://unity.com/download
2. Add project from disk → `openworldgame` (не `My project/`)
3. Дождись компиляции — сцена `Assets/Scenes/Main.unity` соберётся сама, иначе `OpenWorld → Собрать демо-сцену` → Play

Совместимо с `2022.3 LTS` и `6000+`.

## Структура (41 файл)

```
City/CityGenerator.cs + Visuals/{MaterialLibrary(512/256),Effects}.cs
Vehicle/{CarController(GTA),CarDamage,CarFactory,TrafficCar/Spawner}
Police/{WantedSystem(CrimeType),PoliceCar(мигалка)}
World/{DayNightCycle,Apartment,ChopShop,RaceManager,ATM,WeaponShop,CarShop}
Economy/{PlayerWallet,MoneyPickup} + Save/SaveManager
Jobs/{JobManager,JobMarker,JobGiver} + Entities/{Entity,Pedestrian(страх)}, Pets/{Dog,Cat,PetSpawner}
Weapon/{Weapon,Pistol,Shotgun,Rifle,Grenade,WeaponInventory}
```

## GitHub

https://github.com/olexandrmykhailovskyi-oss/openworldgame — `master` Roslyn 41/41 0 ошибок.
