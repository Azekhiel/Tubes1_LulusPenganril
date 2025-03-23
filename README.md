# Tugas Besar 1 | Strategi Algoritma (IF2211)

Robocode Tank Royale adalah permainan pemrograman di mana pemain membuat kode untuk bot tank yang bertarung dalam pertempuran Battle Royale tanpa kendali langsung. Pemain harus menentukan logika bot, termasuk cara bergerak, mendeteksi lawan, menembak, dan bereaksi terhadap berbagai situasi. Permainan ini bersifat berbasis giliran (_turn-based_), sehingga setiap bot menjalankan perintahnya dalam siklus permainan yang terus berulang. Dalam Tugas Besar 1 untuk mata kuliah Strategi Algoritma (IF2211), dibuat bot-bot tank menggunakan strategi _greedy_ yang akan berkompetisi satu sama lain di Robocode Tank Royale. Digunakan bahasa C# untuk klien (bot), walaupun aplikasi dalam Java.

## Greedy Strategies

Kelompok Lulus Penganril membuat empat bot (satu bot utama dan tiga bot alternatif) berdasarkan strategi greedy sebagai berikut.

1. **ngegasTron**: Bot ini bergerak menjauhi musuh sambil menembak dengan fire power sesuai jarak musuh, atau menabrak musuh jika energi sudah habis dan musuh cukup dekat.
2. **Donat Merah**: Bot ini berusaha terus melingkar menyelamatkan diri sambil menembak bot musuh yang dekatnya.
3. **Avenger**: Bot ini menjauhkan diri dari bot lainnya, namun membalas dendam bot yang menembaknya dengan membunuhnya menggunakan fire power yang terus meningkat.
4. **Asteroid Destroyer**: Bot ini mencari target dengan energi terkecil, lalu mengejar dan menembaknya. Musuh ditabrak jika energinya cukup kecil, dan penembakan juga menggunakan prediksi linier (tanpa backtracking).

## Program Requirements

1. .NET SDK (Software Development Kit)
2. JDK (Java Development Kit)

## Build and Run Program

1. Download `robocode-tankroyale-gui-0.30.0.jar` di link [berikut](https://github.com/Ariel-HS/tubes1-if2211-starter-pack/releases/tag/v1.0).

2. Clone repository ini.

```bash
git clone https://github.com/Azekhiel/Tubes1_LulusPenganril
```

3. (Optional) Download `sample-bots-csharp-0.30.0` di link [berikut](https://github.com/Ariel-HS/tubes1-if2211-starter-pack).

4. Jalankan file `robocode-tankroyale-gui-0.30.0.jar`.

```bash
java -jar robocode-tankroyale-gui-0.30.0.jar
```

3. Klik **Config → Bot Root Directories → Add**, lalu masukkan directory `main-bot` dan `alternative-bots` dari repository clone, serta directory `sample-bots-csharp-0.30.0`.

4. Klik **Battle → Start Battle**, lalu _boot_ dan _add_ bot-bot yang ingin ditandingkan.

5. Klik **Start Battle** untuk memulai permainan.

## Struktur Program

```bash
├── src/
│   ├── main-bot/
│   │   └── AsteroidDestroyer/...
│   └── alternative-bots/
│       ├── ngegasTron/...
│       ├── DonatMerah/...
│       └── Avenger/...
├── doc/
│   └── LulusPenganril.pdf
└── README.md
```

## Authors

1. [Timothy Niels Ruslim (10123053)](https://github.com/timoruslim)
2. [Ghaisan Zaki Pratama (10122078)](https://github.com/GhaisanZP)
3. [William Gerald Briandelo (13222061)](https://github.com/Azekhiel)
