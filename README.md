## Tugas Besar 1 | Strategi Algoritma (IF2211)

Dalam Tugas Besar 1 untuk mata kuliah Strategi Algoritma (IF2211), dibuat bot-bot tank menggunakan strategi greedy yang akan berkompetisi satu sama lain di Robocode Tank Royale. Robocode Tank Royale adalah permainan pemrograman di mana pemain membuat kode untuk bot tank yang bertarung dalam pertempuran Battle Royale tanpa kendali langsung. Pemain harus menentukan logika bot, termasuk cara bergerak, mendeteksi lawan, menembak, dan bereaksi terhadap berbagai situasi.

# Greedy Strategies

Kelompok Lulus Penganril membuat empat bot (satu bot utama dan tiga bot alternatif) berdasarkan strategi greedy sebagai berikut.

1. ngegasTron: Bot ini bergerak menjauh musuh sambil menembak dengan energi sesuai jarak musuh, atau menabrak musuh jika energi habis dan musuh cukup dekat.
2. Donat Merah: Bot ini berusaha terus melingkar menyelamatkan diri sambil menembak bot musuh yang dekat.
3. Avenger: Bot ini menjauhkan diri dari bot lainnya, namun membalas dendam bot yang menembaknya dengan membunuhnya menggunakan fire power yang terus meningkat.
4. Asteroid Destroyer: Bot ini mencari target dengan energi terkecil, lalu mengejar dan menembaknya. Musuh ditabrak jika energinya cukup kecil, dan penembakan juga menggunakan prediksi linier (tanpa backtracking).

# Struktur Program

Digunakan bahasa C# untuk client (bot) dan Java untuk game-engine. Maka, diperlukan kedua .NET SDK (Software Development Kit) dan JDK (Java Development Kit) perlu terinstall. Jika sudah ada, dan aplikasi Robocode Tankroyale sudah siap, berikut langkah-langkah untuk menjalankan bots di permainan.

1. Clone repository.

```bash
git clone https://github.com/Azekhiel/Tubes1_LulusPenganril
```

2. Jalankan file .jar aplikasi Robocode Tank Royale.

```bash
java -jar robocode-tankroyale-gui-0.30.0.jar
```

3. Klik “Config" → “Bot Root Directories”, lalu masukkan directory `main-bot` dan `alternative-bots` dari repository clone.

4. Klik "Battle" → "Start Battle", lalu boot dan add bot-bot ngegasTron, Donat Merah, Avenger, dan Asteroid Destroyer sesuai keinginan.

5. Klik "Start Battle" untuk memulai permainan.

Adapun struktur repository sebagai berikut.

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

# Authors

1. Timothy Niels Ruslim (10123053)
2. Ghaisan Zaki Pratama (10122078)
3. William Gerald Briandelo (13222061)
