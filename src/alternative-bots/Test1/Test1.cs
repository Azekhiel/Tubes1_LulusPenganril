using System;
using System.Drawing;
using Robocode.TankRoyale.BotApi;
using Robocode.TankRoyale.BotApi.Events;

public class Test1 : Bot
{
    int turnDirection = 1;
    bool movingForward;
    bool enemyDetected = false; 
    bool paused = false;

    static void Main() => new Test1().Start();

    Test1() : base(BotInfo.FromFile("Test1.json")){
    }

    private void ResetState(){
        paused = false;
        enemyDetected = false;
        movingForward = true;
    }
    public override void OnGameStarted(GameStartedEvent e){
        ResetState();
    }    

    public override void OnRoundStarted(RoundStartedEvent e){
        ResetState();
    }

    private void AimTarget(double x, double y){
        var bearing = BearingTo(x, y);
        if (bearing >= 0)
        {
            turnDirection = 1;
            TurnGunLeft(bearing);
        }
        else
        {
            turnDirection = -1;
            TurnGunRight(-bearing);
        }
        WaitFor(new TurnCompleteCondition(this));
    }

    private void StopMovement() => Stop();

    private void ResumeMovement()
    {
        paused = false;
        enemyDetected = false;
    }

    public override void Run()
    {
        BodyColor   = Color.FromArgb(0x00, 0xC8, 0x00); // lime
        TurretColor = Color.FromArgb(0x00, 0x96, 0x32); // green
        RadarColor  = Color.FromArgb(0x00, 0x64, 0x64); // dark cyan
        BulletColor = Color.FromArgb(0xFF, 0xFF, 0x64); // yellow
        ScanColor   = Color.FromArgb(0xFF, 0xC8, 0xC8); // light red

        movingForward = true;

        while (IsRunning)
        {
            if (enemyDetected==false && paused==false)
            {
                SetTurnLeft(5);
                SetTurnGunLeft(5);
                SetForward(300);
            }
            Go();
        }
    }

    public override void OnScannedBot(ScannedBotEvent e)
    {
        paused = true;
        enemyDetected = true;
        StopMovement();

        double distance = DistanceTo(e.X, e.Y);

        if (distance <= 50)
        {
            AimTarget(e.X, e.Y);
            WaitFor(new TurnCompleteCondition(this));
            Fire(Energy * 0.75);
            WaitFor(new TurnCompleteCondition(this));
        }
        else if (Energy >= 30 && distance <= 1000)
        {
            AimTarget(e.X, e.Y);
            WaitFor(new TurnCompleteCondition(this));
            double firePower = Math.Max(1, 5 - (distance / 200));
            Fire(firePower);
            WaitFor(new TurnCompleteCondition(this));
        }
        else if (Energy < 30 && distance <= 300)
        {
            AimTarget(e.X, e.Y);
            WaitFor(new TurnCompleteCondition(this));
            SetForward(200);
            WaitFor(new TurnCompleteCondition(this));
        }
        
        ResumeMovement();
    }

    public override void OnHitWall(HitWallEvent e)
    {
        ReverseDirection();
    }

    public override void OnHitBot(HitBotEvent e)
    {
        if (e.IsRammed)
        {
            ReverseDirection();
        }
    }

    public void ReverseDirection()
    {
        if (movingForward)
        {
            SetBack(40000);
            movingForward = false;
        }
        else
        {
            SetForward(40000);
            movingForward = true;
        }
    }
}

public class TurnCompleteCondition : Condition
{
    private readonly Bot bot;

    public TurnCompleteCondition(Bot bot)
    {
        this.bot = bot;
    }

    public override bool Test() => bot.TurnRemaining == 0;
}
