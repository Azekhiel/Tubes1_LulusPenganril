using System;
using System.Drawing;
using Robocode.TankRoyale.BotApi;
using Robocode.TankRoyale.BotApi.Events;

public class Circle : Bot
{   
    double centerX;
    double centerY;
    double minDim;
    double radius;
    bool initialized = false;

    // Parameter gerakan
    const double WALL_MARGIN = 50;     // margin agar tidak menabrak dinding

    // Faktor koreksi untuk menjaga radius
    const double RADIUS_CORRECTION_FACTOR = 0.2;
    const double RADIUS_TOLERANCE = 5; // toleransi error radius

    // Radar: berputar perlahan
    const double RADAR_TURN_STEP = 5;

    /* A bot that drives forward and backward, and fires a bullet */
    static void Main(string[] args)
    {
        new Circle().Start();
    }

    Circle() : base(BotInfo.FromFile("Circle.json")) { 
    }

    // Helper untuk memindahkan bot ke koordinat target secara asinkron
    private void MoveTo(double targetX, double targetY)
    {
        double bearing = BearingTo(targetX, targetY);
        Console.WriteLine($"Turning to {targetX:F1}, {targetY:F1} with bearing {bearing:F1}");
        SetTurnRight(bearing);
        Go();
        double distance = DistanceTo(targetX, targetY);
        Console.WriteLine($"Moving forward {distance:F1} units");
        SetForward(distance);
        Go();
    }
    public override void Run()
    {
        /* Customize bot colors, read the documentation for more information */
    var blue = Color.FromArgb(0x00, 0x00, 0xFF);
    BodyColor = blue;
    TurretColor = blue;
    RadarColor = blue;
    ScanColor = blue;
    BulletColor = blue;
    AdjustGunForBodyTurn = true;
    AdjustRadarForBodyTurn = false;
    AdjustRadarForGunTurn = false;
    
    if (!initialized)
    {
        centerX = ArenaWidth / 2.0;
        centerY = ArenaHeight / 2.0;
        minDim = Math.Min(ArenaWidth, ArenaHeight);
        radius = 0.5 * (minDim / 2.0);
        Console.WriteLine($"Initialized: Center=({centerX:F1}, {centerY:F1}), Radius={radius:F1}");


        // Hitung keempat titik pada tepi lingkaran dengan sudut 0°, 90°, 180°, 270°
        // 0°: kanan, 90°: atas, 180°: kiri, 270°: bawah 
        PointF[] candidates = new PointF[4];
        candidates[0] = new PointF((float)(centerX + radius), (float)(centerY));         // 0°
        candidates[1] = new PointF((float)(centerX), (float)(centerY - radius));         // 90°
        candidates[2] = new PointF((float)(centerX - radius), (float)(centerY));         // 180°
        candidates[3] = new PointF((float)(centerX), (float)(centerY + radius));         // 270°

        // Hitung jarak dari posisi bot saat ini (X, Y) ke tiap candidate
        double minDistance = double.MaxValue;
        int minIndex = 0;
        for (int i = 0; i < candidates.Length; i++)
        {
            double dx = candidates[i].X - X;
            double dy = candidates[i].Y - Y;
            double d = Math.Sqrt(dx * dx + dy * dy);
            Console.WriteLine($"Candidate {i}: ({candidates[i].X:F1}, {candidates[i].Y:F1}), distance = {d:F1}");
            if (d < minDistance)
            {
                minDistance = d;
                minIndex = i;
            }
        }

        Console.WriteLine($"Closest candidate is index {minIndex} with distance {minDistance:F1}");
        
        // Pindahkan bot ke titik candidate terdekat
        MoveTo(candidates[minIndex].X, candidates[minIndex].Y);
        initialized = true;
    }

        while (IsRunning)
        {
        AvoidWallsInLoop();
        // Hitung sudut ke pusat
        double angleToCenter = DirectionTo(centerX, centerY);
        double desiredHeading = NormalizeRelativeAngle(angleToCenter - 90);
        double headingError = NormalizeRelativeAngle(desiredHeading - Direction);
        SetTurnRight(headingError);

        // Hitung koreksi jarak agar tetap pada radius
        double currentDistance = DistanceTo(centerX, centerY);
        double distanceError = currentDistance - radius;
        double correction = 0;
        if (Math.Abs(distanceError) > RADIUS_TOLERANCE)
        {
            correction = -distanceError * RADIUS_CORRECTION_FACTOR;
            correction = Math.Max(Math.Min(correction, 50), -50);
        }
        
        SetForward(correction);

        // Radar berputar perlahan
        SetTurnRadarRight(RADAR_TURN_STEP);

        Go();
        AvoidWallsInLoop();
        }
    }
    public override void OnScannedBot(ScannedBotEvent e)
    {
    // Hitung jarak ke target saat ini
    double targetDistance = Math.Sqrt(Math.Pow(e.X - X, 2) + Math.Pow(e.Y - Y, 2));
    
    // firepower berdasarkan jarak 
    double firePower = (targetDistance < 2 * radius) ? 3 : 1;

    if (e.Energy < 20) 
    {
        SetForward(targetDistance + 50);
        Fire(firePower);
        Go();
        RepositionToCircle();
    }    
    else {
        double radarDirection = RadarBearingTo(e.X, e.Y);
        double gunDirection = GunBearingTo(e.X, e.Y);
        
        // Terapkan faktor smoothing untuk perputaran radar dan gun
        double smoothingFactor = 0.5;
        double adjustedRadarTurn = smoothingFactor * radarDirection;
        double adjustedGunTurn = smoothingFactor * gunDirection;
        
        SetTurnRadarLeft(adjustedRadarTurn);
        SetTurnGunLeft(adjustedGunTurn);
        
        // Jika target berada dalam jarak tertentu, tembak
        if (targetDistance < 2 * radius)
        {
            Fire(firePower);
        }
        
        Go();        
        Console.WriteLine("I see a bot at " + e.X + ", " + e.Y);  
    }  
    }
    public void RepositionToCircle()
    {
        // Hitung keempat titik pada tepi lingkaran dengan sudut 0°, 90°, 180°, 270°
        // 0°: kanan, 90°: atas, 180°: kiri, 270°: bawah 
        PointF[] candidates = new PointF[4];
        candidates[0] = new PointF((float)(centerX + radius), (float)(centerY));         // 0°
        candidates[1] = new PointF((float)(centerX), (float)(centerY - radius));         // 90°
        candidates[2] = new PointF((float)(centerX - radius), (float)(centerY));         // 180°
        candidates[3] = new PointF((float)(centerX), (float)(centerY + radius));         // 270°

        // Hitung jarak dari posisi bot saat ini (X, Y) ke tiap candidate
        double minDistance = double.MaxValue;
        int minIndex = 0;
        for (int i = 0; i < candidates.Length; i++)
        {
            double dx = candidates[i].X - X;
            double dy = candidates[i].Y - Y;
            double d = Math.Sqrt(dx * dx + dy * dy);
            Console.WriteLine($"Candidate {i}: ({candidates[i].X:F1}, {candidates[i].Y:F1}), distance = {d:F1}");
            if (d < minDistance)
            {
                minDistance = d;
                minIndex = i;
            }
        }

        Console.WriteLine($"Closest candidate is index {minIndex} with distance {minDistance:F1}");
        
        // Pindahkan bot ke titik candidate terdekat
        MoveTo(candidates[minIndex].X, candidates[minIndex].Y);

    }

    public override void OnHitBot(HitBotEvent e)
    {
        Console.WriteLine("Ouch! I hit a bot at " + e.X + ", " + e.Y);
    
    }
    public override void OnHitWall(HitWallEvent e)
    {
        Console.WriteLine("Ouch! I hit a wall, must turn back!");
        AvoidWallsInLoop();
    }
    private void AvoidWallsInLoop()
    {
        if (X < WALL_MARGIN)
        {
            // Jauh dari dinding kiri
            SetStop();
            Go();
            SetBack(100);
            SetTurnRight(90);
            Go();
        }
        else if (X > ArenaWidth - WALL_MARGIN)
        {
            // Jauh dari dinding kanan
            SetStop();
            Go();
            SetBack(100);
            SetTurnLeft(90);
            Go();
        }

        if (Y < WALL_MARGIN)
        {
            // Dinding atas
            SetStop();
            Go();
            SetBack(100);
            SetTurnRight(90);
            Go();
        }
        else if (Y > ArenaHeight - WALL_MARGIN)
        {
            // Dinding bawah
            SetStop();
            Go();
            SetBack(100);
            SetTurnLeft(90);
            Go();
        }
    }
    /* Read the documentation for more events and methods */
}
