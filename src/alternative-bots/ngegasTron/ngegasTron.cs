using System;
using System.Drawing;
using Robocode.TankRoyale.BotApi;
using Robocode.TankRoyale.BotApi.Events;

public class ngegasTron : Bot
{
    bool movingForward;
    bool enemyDetected = false; 
    bool paused = false;

    static void Main() => new ngegasTron().Start();

    ngegasTron() : base(BotInfo.FromFile("ngegasTron.json")){
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
            TurnGunLeft(bearing);
        }
        else
        {
            TurnGunRight(-bearing);
        }
        WaitFor(new TurnCompleteCondition(this));
    }

    private void StopMovement() => Stop();

    private void ResumeMovement(){
        paused = false;
        enemyDetected = false;
    }

    public override void Run(){
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
                SetTurnLeft(16);
                SetTurnGunLeft(10);
                SetForward(300);
            }
            Go();
        }
    }

    public override void OnScannedBot(ScannedBotEvent e){
        paused = true;
        enemyDetected = true;
        StopMovement();
        double a,b;//sebagai konstanta untuk mengatasi pergerakan musuh dan lokasi penembakan awal yang tidak sesuai
        SetTurnGunLeft(-12);
        if (e.X>=0){
            a=6;
        }
        else {
            a=-6;
        }
        if(e.Y>=0){
            b=6;
        }
        else{
            b=-6;
        }

        double distance = DistanceTo(e.X, e.Y);

        if (distance <= 30)
        {   

            AimTarget(e.X+a, e.Y+b);
            Fire(Energy * 0.75);
            WaitFor(new TurnCompleteCondition(this));
        }
        else if (Energy >= 30 && distance <= 1000)
        {
            AimTarget(e.X+a, e.Y+b);
            Forward(200);
            Fire(5 - (distance / 400));
            WaitFor(new TurnCompleteCondition(this));
        }
        else if (Energy < 30)
        {
            AimTarget(e.X, e.Y);
            WaitFor(new TurnCompleteCondition(this));
            SetTurnLeft(-16);
            SetForward(200);
            Fire(1);
            WaitFor(new TurnCompleteCondition(this));
        }
        
        ResumeMovement();
    }

    public override void OnHitWall(HitWallEvent e){
        ReverseDirection();
    }

    public override void OnHitBot(HitBotEvent e){
        if (e.IsRammed)
        {
            ReverseDirection();
        }
    }

    public void ReverseDirection(){
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

public class TurnCompleteCondition : Condition{
    private readonly Bot bot;

    public TurnCompleteCondition(Bot bot)
    {
        this.bot = bot;
    }

    public override bool Test() => bot.TurnRemaining == 0;
}
