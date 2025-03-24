using System;
using System.Drawing;
using Robocode.TankRoyale.BotApi;
using Robocode.TankRoyale.BotApi.Events;

public class AsteroidDestroyer : Bot
{  
    int ScannedTurn = 0;

    int TargetId = -1;
    double TargetEnergy;

    sbyte Polarity = 1;

    enum BotMode { Scanning, Chasing, Strafing }
    BotMode Mode = BotMode.Scanning;
    int ModeCooldown = 0;


    static void Main(string[] args)
    {
        new AsteroidDestroyer().Start();
    }

    AsteroidDestroyer() : base(BotInfo.FromFile("AsteroidDestroyer.json")) { }
    
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

        ScannedTurn = 0;

        while (IsRunning)
        {
            Mode = (TurnNumber - ScannedTurn > 1) ? BotMode.Scanning : Mode; // Go to search mode if lost target

            if (Mode == BotMode.Scanning) // Actively search for target by spinning and moving around
            {
                TargetId = -1;
                SetTurnRadarLeft(double.PositiveInfinity);
                SetTurnLeft(BearingTo(ArenaWidth/2, ArenaHeight/2));
                SetForward(100); 
            } 
            
            Go();
        }
    }
    
    public override void OnScannedBot(ScannedBotEvent e)
    {   
        if (TargetId < 0) // If no target yet, make this our new target
        {
            TargetId = e.ScannedBotId;
            TargetEnergy = e.Energy;
        }
            
        if (TargetId == e.ScannedBotId) // If this is our target
        {
            ScannedTurn = TurnNumber;
            TargetEnergy = e.Energy;

            // Calculate Distance
            double TargetDistance = Math.Sqrt(Math.Pow(e.X - X, 2) + Math.Pow(e.Y - Y, 2)); 

            // Calculate Fire Power
            double DistanceFactor = 3 * Math.Exp(-TargetDistance / 250);
            double SpeedFactor = 1 - (e.Speed / 8); 
            double EnergyFactor = 1 - Math.Exp(-Energy / 25);
            double FirePower = (DistanceFactor + SpeedFactor) * EnergyFactor; 

            // Predict Location
            PointF TargetPosition = LinearPrediction(e, FirePower); 

            // Calculate Directions
            double RadarDirection = RadarBearingTo(e.X, e.Y);   
            double GunDirection = GunBearingTo(TargetPosition.X, TargetPosition.Y);
            double MoveDirecion = BearingTo(TargetPosition.X, TargetPosition.Y);

            // Move Bot
            SetTurnRadarLeft(RadarDirection);
            SetTurnGunLeft(GunDirection); 
            Fire(FirePower);

            // Switching Modes Using Cooldown (to fix bug with being stuck)
            if (ModeCooldown > 15) 
            {
                ModeCooldown = 0;
                Mode = (TargetDistance > 150 || e.Energy < 25) ? BotMode.Chasing : BotMode.Strafing;
            } else 
            {
                ModeCooldown++;         
            }

            if (Mode == BotMode.Chasing) // Chase if far
            {
                SetTurnLeft(MoveDirecion);   
                SetForward(TargetDistance / 2);
            }   
            else // Strafe if close
            {
                SetTurnLeft(MoveDirecion + 90); 
                SetForward(Polarity * 100); 
            }

            if (RadarDirection == 0) // Rescan if straight ahead
                Rescan(); 
        
        } else if (e.Energy < TargetEnergy) // If this bot has less energy than current target, make this our new target
        {
            TargetId = e.ScannedBotId;
            TargetEnergy = e.Energy;
        }       
    }

    public override void OnBotDeath(BotDeathEvent e)
    {
        if (e.VictimId == TargetId)
            TargetId = -1;
    }

    public override void OnHitBot(HitBotEvent e)
    {
        Polarity *= -1; 
        SetBack(100);
        SetTurnRadarLeft(RadarBearingTo(e.X, e.Y) + 90);
        if (e.Energy < TargetEnergy) // If this bot has less energy than current target, make this our new target
        {
            TargetId = e.VictimId;
            TargetEnergy = e.Energy;
            SetTurnRadarLeft(RadarBearingTo(e.X, e.Y));
        }  
        Go();
    }

    public override void OnHitWall(HitWallEvent e)
    {
        Polarity *= -1; // Make bot go the other way
    }

    public PointF LinearPrediction(ScannedBotEvent scannedBot, double firePower)
    {

        double bulletSpeed = 20 - (3 * firePower);
        double predictedX = scannedBot.X;
        double predictedY = scannedBot.Y;
        double enemyDirection = Math.PI * scannedBot.Direction / 180.0;

        double prevDistance = double.MaxValue;

        // Iterative Method (Similar to Newton's Method)
        for (int i = 0; i < 30; i++) 
        {
            double distance = Math.Sqrt(Math.Pow(predictedX - X, 2) + Math.Pow(predictedY - Y, 2));
            double time = distance / bulletSpeed;

            if (Math.Abs(distance - prevDistance) < 1.0) 
                break;

            prevDistance = distance;

            predictedX = scannedBot.X + Math.Cos(enemyDirection) * scannedBot.Speed * time;
            predictedY = scannedBot.Y + Math.Sin(enemyDirection) * scannedBot.Speed * time;
        }

        return new PointF((float) predictedX, (float) predictedY);
    }

}
