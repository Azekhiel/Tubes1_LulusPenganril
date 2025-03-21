using System;
using System.Drawing;
using Robocode.TankRoyale.BotApi;
using Robocode.TankRoyale.BotApi.Events;

public class Avenger : Bot
{  
    int lastScannedTick = 0;

    int targetId = -1;
    (double X, double Y) targetLocation;
    int rageFactor = 0;

    enum BotMode { Search, Revenge }
    BotMode mode = BotMode.Search;


    static void Main(string[] args)
    {
        new Avenger().Start();
    }

    Avenger() : base(BotInfo.FromFile("Avenger.json")) { }
    
    public override void Run()
    {
        BodyColor = Color.FromArgb(30, 144, 255); 
        TurretColor = Color.FromArgb(255, 59, 48); 
        RadarColor = Color.White; 
        ScanColor = Color.FromArgb(224, 224, 224); 
        BulletColor = Color.FromArgb(255, 59, 48);

        AdjustGunForBodyTurn = true;
        AdjustRadarForBodyTurn = true;
        AdjustRadarForGunTurn = true;

        targetId = -1;
        lastScannedTick = 0;

        while (IsRunning)
        {
            
            if (targetId < 0 || TurnNumber - lastScannedTick > 1) 
            {
                targetId = -1;
                mode = BotMode.Search;
                SetTurnRadarLeft(double.PositiveInfinity);
            } 

            if (targetId > 0 && mode == BotMode.Search)
            {
                SetTurnRadarLeft(double.PositiveInfinity);
                SetTurnLeft(BearingTo(targetLocation.X, targetLocation.Y));
                SetForward(100); 
            }
            
            Go();
        }
    }
    
    public override void OnScannedBot(ScannedBotEvent e)
    {  
        if (targetId < 0)
        {
            SetTurnLeft(-GunBearingTo(e.X, e.Y));
            SetForward(100);
        }
        if (targetId == e.ScannedBotId)
        {
            mode = BotMode.Revenge;
            lastScannedTick = TurnNumber;

            double targetDistance = Math.Sqrt(Math.Pow(e.X - X, 2) + Math.Pow(e.Y - Y, 2));
            double firePower = 4 * Math.Exp(-targetDistance / (250 + rageFactor));
            PointF targetPosition = LinearPrediction(e, targetDistance / (20 - (3 * firePower))); 

            double radarDirection = RadarBearingTo(e.X, e.Y);   
            double gunDirection = GunBearingTo(targetPosition.X, targetPosition.Y);
            double moveDirection = BearingTo(targetPosition.X, targetPosition.Y);

            SetTurnRadarLeft(radarDirection);
            SetTurnGunLeft(gunDirection); 
            Fire(firePower);

            if (targetDistance > 50) 
            {
                SetTurnLeft(moveDirection);
                SetForward(targetDistance / 2);
            } 

            if (radarDirection == 0)
                Rescan(); 
        }
    }

    public override void OnHitByBullet(HitByBulletEvent e)
    {
        if (targetId < 0) {
            lastScannedTick = TurnNumber;       
            targetId = e.Bullet.OwnerId;
            targetLocation = (e.Bullet.X, e.Bullet.Y);
            SetTurnRadarLeft(RadarBearingTo(e.Bullet.X, e.Bullet.Y));
            SetTurnGunLeft(GunBearingTo(e.Bullet.X, e.Bullet.Y));
            SetForward(100);
        }
    }

    public override void OnBotDeath(BotDeathEvent e)
    {
        if (e.VictimId == targetId)
            targetId = -1;
            rageFactor = 0;
            mode = BotMode.Search;
    }

    public override void OnHitBot(HitBotEvent e)
    {
        if (targetId < 0) {
            lastScannedTick = TurnNumber;       
            targetId = e.VictimId;
            targetLocation = (e.X, e.Y);
            SetTurnRadarLeft(RadarBearingTo(e.X, e.Y));
            SetTurnGunLeft(GunBearingTo(e.X, e.Y));
            SetForward(100);
        }
        SetTurnLeft(BearingTo(e.X, e.Y) + 90);  
        SetBack(100); 
        Go();
    }

    public override void OnBulletHit(BulletHitBotEvent e)
    {
        if (e.VictimId == targetId)
            rageFactor += 100;
        else
            rageFactor = 0;
    }

    public PointF LinearPrediction(ScannedBotEvent scannedBot, double time)
    {
        double theta = Math.PI * scannedBot.Direction / 180.0;
        
        double predictedX = scannedBot.X + Math.Cos(theta) * scannedBot.Speed * time;
        double predictedY = scannedBot.Y + Math.Sin(theta) * scannedBot.Speed * time;

        return new PointF((float) predictedX, (float) predictedY);
    }

}
