using System;
using System.Drawing;
using Robocode.TankRoyale.BotApi;
using Robocode.TankRoyale.BotApi.Events;

public class AsteroidDestroyer : Bot
{  
    Random random = new Random();
    int currentTick = 0;
    int lastScannedTick = 0;

    string targetId = null;
    int chips = 0;

    int bitDirection = 1;
    string mode = "Chasing";
    int modeTick = 0;

    static void Main(string[] args)
    {
        new AsteroidDestroyer().Start();
    }

    AsteroidDestroyer() : base(BotInfo.FromFile("AsteroidDestroyer.json")) { }
    
    // Called when a new round is started -> initialize and do some movement
    public override void Run()
    {
        var pink = Color.FromArgb(0xFF, 0x69, 0xB4);
        BodyColor = pink;
        TurretColor = pink;
        RadarColor = pink;
        ScanColor = pink;
        BulletColor = pink;

        AdjustGunForBodyTurn = true;
        AdjustRadarForBodyTurn = true;
        AdjustRadarForGunTurn = true;

        // Loop while running
        while (IsRunning)
        {
            currentTick++;      

            if (targetId == null || currentTick - lastScannedTick > 30) 
            {
                targetId = null;
                SetTurnRadarLeft(Double.PositiveInfinity);
                SetTurnLeft(BearingTo(ArenaWidth/2, ArenaHeight/2) + RandomGaussian(0, 90));
                SetForward(100); 
            } 
            
            Go();
        }
    }
    
    public override void OnScannedBot(ScannedBotEvent e)
    {   
        if (targetId == null)
        {
            targetId = e.ScannedBotId.ToString();
            
        }
            
        if (targetId == e.ScannedBotId.ToString())
        {
            lastScannedTick = currentTick;

            double targetDistance = Math.Sqrt(Math.Pow(e.X - X, 2) + Math.Pow(e.Y - Y, 2));
            double firePower = 3 * Math.Exp(-targetDistance / (250 + chips)) * Energy/100;
            PointF targetPosition = LinearPrediction(e, targetDistance / (20 - (3 * firePower))); 

            double radarDirection = RadarBearingTo(e.X, e.Y);   
            double gunDirection = GunBearingTo(targetPosition.X, targetPosition.Y);
            double moveDirecion = BearingTo(targetPosition.X, targetPosition.Y);

            SetTurnRadarLeft(radarDirection);
            SetTurnGunLeft(gunDirection); 

            if (modeTick > 15) 
            {
                modeTick = 0;
                mode = (targetDistance > 150) ? "Chasing" : "Strafing";
            } else 
            {
                modeTick++;
            }

            if (mode == "Chasing") 
            {
                int mult = (X < 20 || X > ArenaWidth - 20 || Y < 20 || Y > ArenaHeight - 20) ? 0 : 1;
                SetTurnLeft(moveDirecion + mult * RandomGaussian(0, 90));   
                SetForward(targetDistance / 2);
            }   
            else 
            {
                SetTurnLeft(moveDirecion + RandomGaussian(90, 30)); 
                SetForward(bitDirection * 100); 
            }
            Fire(firePower);

            if (radarDirection == 0)
                Rescan(); 
        }
    }

    public override void OnWonRound(WonRoundEvent e)
    {
        for (int i = 0; i < 10; i++)
        {
            BodyColor = Color.FromArgb(random.Next(256), random.Next(256), random.Next(256));
            GunColor = Color.FromArgb(random.Next(256), random.Next(256), random.Next(256));
            RadarColor = Color.FromArgb(random.Next(256), random.Next(256), random.Next(256));

            SetTurnGunRight(Double.PositiveInfinity);
            SetTurnRadarLeft(Double.PositiveInfinity);

            SetForward(100);
            if (i % 2 == 0)
                SetTurnRight(RandomGaussian(180, 90)); 
            else
                SetTurnLeft(RandomGaussian(180, 90));
            
            Fire(0.1);
            Go();
        }
    }

    public override void OnBotDeath(BotDeathEvent e)
    {
        if (e.VictimId.ToString() == targetId)
            targetId = null;
            chips = 0;
    }

    public override void OnHitBot(HitBotEvent e)
    {
        bitDirection = -bitDirection; 
        SetTurnLeft(BearingTo(e.X, e.Y) + RandomGaussian(90, 30));
        SetForward(100);
        Go();
    }

    public override void OnHitWall(HitWallEvent e)
    {
        bitDirection = -bitDirection;
    }

    public override void OnBulletHit(BulletHitBotEvent e)
    {
        if (e.VictimId.ToString() == targetId)
            chips += 100;
        else
            chips = 0;
    }

    public PointF LinearPrediction(ScannedBotEvent scannedBot, double turnsAhead)
    {
        double targetDirection = Math.PI * scannedBot.Direction / 180.0;
        
        float futureX = (float)(scannedBot.X + Math.Cos(targetDirection) * scannedBot.Speed * turnsAhead);
        float futureY = (float)(scannedBot.Y + Math.Sin(targetDirection) * scannedBot.Speed * turnsAhead);

        return new PointF(futureX, futureY);
    }
    
    double RandomGaussian(double mu, double sd)
    {   
        // Thanks to yoyoyoyosef for the method: https://stackoverflow.com/a/218600/17651489
        double u1 = 1.0 - random.NextDouble(); 
        double u2 = 1.0 - random.NextDouble();
        double stdNormal = Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Sin(2.0 * Math.PI * u2);
        return mu + sd * stdNormal; 
    }

}
