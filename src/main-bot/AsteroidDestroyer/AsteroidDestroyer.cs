using System;
using System.Drawing;
using Robocode.TankRoyale.BotApi;
using Robocode.TankRoyale.BotApi.Events;

public class AsteroidDestroyer : Bot
{  
    int currentTick = 0;
    int lastScannedTick = 0;

    string targetId = null;
    int chips = 0;
    int bitDirection = 1;

    enum BotMode { Scanning, Chasing, Strafing, Evading }
    BotMode mode = BotMode.Scanning;
    
    int modeCooldown = 0;


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
            Console.WriteLine(targetId);

            if (targetId == null || currentTick - lastScannedTick > 15) 
            {
                targetId = null;
                SetTurnRadarLeft(Double.PositiveInfinity);
                SetTurnLeft(BearingTo(ArenaWidth/2, ArenaHeight/2));
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
            double firePower = 4 * Math.Exp(-targetDistance / (250 + chips)) * Energy/100;
            PointF targetPosition = LinearPrediction(e, targetDistance / (20 - (3 * firePower))); 

            double radarDirection = RadarBearingTo(e.X, e.Y);   
            double gunDirection = GunBearingTo(targetPosition.X, targetPosition.Y);
            double moveDirecion = BearingTo(targetPosition.X, targetPosition.Y);

            SetTurnRadarLeft(radarDirection);
            SetTurnGunLeft(gunDirection); 
            Fire(firePower);

            if (modeCooldown > 15) 
            {
                modeCooldown = 0;
                mode = (targetDistance > 150) ? BotMode.Chasing : BotMode.Strafing;
            } else 
            {
                modeCooldown++;
            }

            if (mode == BotMode.Chasing) 
            {
                SetTurnLeft(moveDirecion);   
                SetForward(targetDistance / 2);
            }   
            else 
            {
                SetTurnLeft(moveDirecion + 90); 
                SetForward(bitDirection * 100); 
            }
            Console.WriteLine("Walking");

            if (radarDirection == 0)
                Rescan(); 
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
        Console.WriteLine("Dodging");
        bitDirection = -bitDirection; 
        SetTurnLeft(BearingTo(e.X, e.Y));  
        SetBack(100);
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

    public PointF LinearPrediction(ScannedBotEvent scannedBot, double time)
    {
        double theta = Math.PI * scannedBot.Direction / 180.0;
        
        float predictedX = (float)(scannedBot.X + Math.Cos(theta) * scannedBot.Speed * time);
        float predictedY = (float)(scannedBot.Y + Math.Sin(theta) * scannedBot.Speed * time);

        return new PointF(predictedX, predictedY);
    }

}
