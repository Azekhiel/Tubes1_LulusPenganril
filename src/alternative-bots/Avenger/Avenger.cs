using System;
using System.Drawing;
using Robocode.TankRoyale.BotApi;
using Robocode.TankRoyale.BotApi.Events;

public class Avenger : Bot
{  
    int lastScannedTick = 0;

    string targetId = null;
    int rageFactor = 0;
    int bitDirection = 1;

    enum BotMode { Scanning, Chasing, Strafing }
    BotMode mode = BotMode.Scanning;
    int modeCooldown = 0;


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

        lastScannedTick = 0;

        while (IsRunning)
        {

            if (targetId == null || TurnNumber - lastScannedTick > 1) 
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
            lastScannedTick = TurnNumber;

            double targetDistance = Math.Sqrt(Math.Pow(e.X - X, 2) + Math.Pow(e.Y - Y, 2));
            double firePower = 4 * Math.Exp(-targetDistance / (250 + rageFactor));
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

            if (radarDirection == 0)
                Rescan(); 
        }
    }

    public override void OnBotDeath(BotDeathEvent e)
    {
        if (e.VictimId.ToString() == targetId)
            targetId = null;
            rageFactor = 0;
    }

    public override void OnHitBot(HitBotEvent e)
    {
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
            rageFactor += 100;
        else
            rageFactor = 0;
    }

    public PointF LinearPrediction(ScannedBotEvent scannedBot, double time)
    {
        double theta = Math.PI * scannedBot.Direction / 180.0;
        
        float predictedX = (float)(scannedBot.X + Math.Cos(theta) * scannedBot.Speed * time);
        float predictedY = (float)(scannedBot.Y + Math.Sin(theta) * scannedBot.Speed * time);

        return new PointF(predictedX, predictedY);
    }

}
