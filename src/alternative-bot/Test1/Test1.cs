using System;
using System.Drawing;
using Robocode.TankRoyale.BotApi;
using Robocode.TankRoyale.BotApi.Events;

public class Test1 : Bot
{
    int turnDirection = 1; 
    bool movingForward;
    bool adaMusuh = false;
    int musuhHilang = 1;

    private void TurnToFaceTarget(double x, double y)
    {
        var bearing = BearingTo(x, y);
        if (bearing >= 0)
            turnDirection = 1;
        else
            turnDirection = -1;

        TurnLeft(bearing);
    }

    static void Main()
    {
        new Test1().Start();
    }

    // Constructor, which loads the bot config file
    Test1() : base(BotInfo.FromFile("Test1.json")) { }

    // Called when a new round is started -> initialize and do some movement
    public override void Run()
    {
        BodyColor = Color.FromArgb(0x00, 0xC8, 0x00);   // lime
        TurretColor = Color.FromArgb(0x00, 0x96, 0x32); // green
        RadarColor = Color.FromArgb(0x00, 0x64, 0x64);  // dark cyan
        BulletColor = Color.FromArgb(0xFF, 0xFF, 0x64); // yellow
        ScanColor = Color.FromArgb(0xFF, 0xC8, 0xC8);   // light red

        movingForward = true;

        // Loop while as long as the bot is running
        while (IsRunning)
        {
            // Tell the game we will want to move ahead 40000 -- some large number
            if (!adaMusuh){
                SetTurnGunLeft(360);
            }
            else{
                musuhHilang++;
                if (musuhHilang>=10){
                    adaMusuh = false;
                    musuhHilang = 0;
                }
            }

            Forward(100);
            movingForward = true;
            Go();
        }
    }

    // We collided with a wall -> reverse the direction
    public override void OnHitWall(HitWallEvent e)
    {
        // Bounce off!
        ReverseDirection();
    }

    // ReverseDirection: Switch from ahead to back & vice versa
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

    // We scanned another bot -> fire!
    public override void OnScannedBot(ScannedBotEvent e) {
        musuhHilang=0;
        adaMusuh=true;
        TurnToFaceTarget(e.X, e.Y);
        var distance = DistanceTo(e.X, e.Y);

        if (distance <= 1000 && Energy>=30){
            Fire(10-(distance/100));
            SetTurnRight(90);
        }
        else if (Energy <30 && distance <=300){
            TurnToFaceTarget(e.X,e.Y);
        }
    }

    // We hit another bot -> back up!
    public override void OnHitBot(HitBotEvent e)
    {
        // If we're moving into the other bot, reverse!
        if (e.IsRammed)
        {
            ReverseDirection();
        }
    }
}

// Condition that is triggered when the turning is complete
public class TurnCompleteCondition : Condition
{
    private readonly Bot bot;

    public TurnCompleteCondition(Bot bot)
    {
        this.bot = bot;
    }

    public override bool Test()
    {
        // turn is complete when the remainder of the turn is zero
        return bot.TurnRemaining == 0;
    }
    
}