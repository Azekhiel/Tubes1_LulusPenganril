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
    const double WALL_MARGIN = 50;
    const double RADIUS_CORRECTION_FACTOR = 0.25;
    const double RADIUS_TOLERANCE = 5;
    const double RADAR_TURN_STEP = 5;

    /* A bot that drives forward and backward, and fires a bullet */
    static void Main(string[] args)
    {
        new Circle().Start();
    }

    Circle() : base(BotInfo.FromFile("Circle.json")) { 
    }

    public override void Run()
    {
    /* Customize bot colors, read the documentation for more information */
    var red = Color.FromArgb(0xFF, 0x00, 0x00);
    BodyColor = red;
    TurretColor = red;
    RadarColor = red;
    ScanColor = red;
    BulletColor = red;
    AdjustGunForBodyTurn = true;
    AdjustRadarForBodyTurn = false;
    AdjustRadarForGunTurn = false;
    
    if (!initialized)
    {
        centerX = ArenaWidth / 2.0;
        centerY = ArenaHeight / 2.0;
        minDim = Math.Min(ArenaWidth, ArenaHeight);
        radius = 0.5 * (minDim / 2.0);

        PointF[] candidates = new PointF[4];
        candidates[0] = new PointF((float)(centerX + radius), (float)(centerY));         
        candidates[1] = new PointF((float)(centerX), (float)(centerY - radius));         
        candidates[2] = new PointF((float)(centerX - radius), (float)(centerY));         
        candidates[3] = new PointF((float)(centerX), (float)(centerY + radius));         

        double minDistance = double.MaxValue;
        int minIndex = 0;
        for (int i = 0; i < candidates.Length; i++)
        {
            double dx = candidates[i].X - X;
            double dy = candidates[i].Y - Y;
            double d = Math.Sqrt(dx * dx + dy * dy);
            if (d < minDistance)
            {
                minDistance = d;
                minIndex = i;
            }
        }        
        MoveTo(candidates[minIndex].X, candidates[minIndex].Y);
        initialized = true;
    }

        while (IsRunning)
        {
        AvoidWallsInLoop();

        double angleToCenter = DirectionTo(centerX, centerY);
        double desiredHeading = NormalizeRelativeAngle(angleToCenter - 90);
        double headingError = NormalizeRelativeAngle(desiredHeading - Direction);
        SetTurnRight(headingError);

        double currentDistance = DistanceTo(centerX, centerY);
        double distanceError = currentDistance - radius;
        double correction = 0;
        if (Math.Abs(distanceError) > RADIUS_TOLERANCE)
        {
            correction = -distanceError * RADIUS_CORRECTION_FACTOR;
            correction = Math.Max(Math.Min(correction, 50), -50);
        }
        
        SetForward(correction);

        SetTurnRadarRight(RADAR_TURN_STEP);
        
        Go();
        AvoidWallsInLoop();
        }
    }

    public override void OnScannedBot(ScannedBotEvent e)
    {

    double targetDistance = Math.Sqrt(Math.Pow(e.X - X, 2) + Math.Pow(e.Y - Y, 2));
    
    double firePower = (targetDistance < 2 * radius) ? 3 : 1;

    if (e.Energy < 20) 
    {
        targetDistance = Math.Sqrt(Math.Pow(e.X - X, 2) + Math.Pow(e.Y - Y, 2));
        firePower = (targetDistance < 2 * radius) ? 3 : 1;
        SetForward(targetDistance + 50);
        double gunDirection = GunBearingTo(e.X, e.Y);        
        Fire(firePower);
        Go();
        RepositionToCircle();
    }    
    else {
        double radarDirection = RadarBearingTo(e.X, e.Y);
        double gunDirection = GunBearingTo(e.X, e.Y);
        
        double smoothingFactor = 0.25;
        double adjustedRadarTurn = smoothingFactor * radarDirection;
        double adjustedGunTurn = smoothingFactor * gunDirection;
        
        SetTurnRadarLeft(adjustedRadarTurn);
        SetTurnGunLeft(adjustedGunTurn);
        Fire(firePower);
        Go();        
    }  
    }
    private void MoveTo(double targetX, double targetY)
    {
        double bearing = BearingTo(targetX, targetY);
        SetTurnRight(bearing);
        Go();
        double distance = DistanceTo(targetX, targetY);
        SetForward(distance);
        Go();
    }
    public void RepositionToCircle()
    {
        PointF[] candidates = new PointF[4];
        candidates[0] = new PointF((float)(centerX + radius), (float)(centerY));         
        candidates[1] = new PointF((float)(centerX), (float)(centerY - radius));         
        candidates[2] = new PointF((float)(centerX - radius), (float)(centerY));         
        candidates[3] = new PointF((float)(centerX), (float)(centerY + radius));         

        double minDistance = double.MaxValue;
        int minIndex = 0;
        for (int i = 0; i < candidates.Length; i++)
        {
            double dx = candidates[i].X - X;
            double dy = candidates[i].Y - Y;
            double d = Math.Sqrt(dx * dx + dy * dy);
            if (d < minDistance)
            {
                minDistance = d;
                minIndex = i;
            }
        }        
        MoveTo(candidates[minIndex].X, candidates[minIndex].Y);
    }
    public override void OnHitBot(HitBotEvent e)
    {
        MoveTo(e.X,e.Y);
        SetTurnLeft(BearingTo(e.X, e.Y) + 90);  
        SetBack(100); 
        Console.WriteLine("Ouch! I hit a bot at " + e.X + ", " + e.Y);
    
    }
    public override void OnHitWall(HitWallEvent e)
    {
        Console.WriteLine("Ouch! I hit a wall, must turn back!");
        AvoidWallsInLoop();
    }
    private void AvoidWallsInLoop()
    {
        bool avoided = false;

        if (X < WALL_MARGIN)
        {
            SetStop();
            SetBack(100);
            SetTurnRight(90);
            avoided = true;
        }
        else if (X > ArenaWidth - WALL_MARGIN)
        {
            SetStop();
            SetBack(100);
            SetTurnLeft(90);
            avoided = true;
        }
        else if (Y < WALL_MARGIN)
        {
            SetStop();
            SetBack(100);
            SetTurnRight(90);
            avoided = true;
        }
        else if (Y > ArenaHeight - WALL_MARGIN)
        {
            SetStop();
            SetBack(100);
            SetTurnLeft(90);
            avoided = true;
        }

        if (avoided)
        {
            Go();
        }
    }

    /* Read the documentation for more events and methods */
}
