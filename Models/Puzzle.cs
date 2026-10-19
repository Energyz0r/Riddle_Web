using System;

namespace Riddle_Web.Models
{
    public static class Sides
    {
        public const string East = "East";
        public const string West = "West";
    }

    public class GameState
    {
        public string Farmer { get; set; }
        public string Fox { get; set; }
        public string Chicken { get; set; }
        public string Corn { get; set; }

        public static GameState Start()
        {
            return new GameState
            {
                Farmer = Sides.East,
                Fox = Sides.East,
                Chicken = Sides.East,
                Corn = Sides.East
            };
        }

        public GameState Clone()
        {
            return new GameState { Farmer = Farmer, Fox = Fox, Chicken = Chicken, Corn = Corn };
        }

        public bool IsValid()
        {
            return IsSide(Farmer) && IsSide(Fox) && IsSide(Chicken) && IsSide(Corn);
        }

        public bool IsWon
        {
            get
            {
                return Farmer == Sides.West && Fox == Sides.West
                    && Chicken == Sides.West && Corn == Sides.West;
            }
        }

        private static bool IsSide(string s)
        {
            return s == Sides.East || s == Sides.West;
        }
    }

    public class MoveRequest
    {
        public GameState State { get; set; }
        public string Passenger { get; set; }
    }

    /// <summary>Status is one of: info, valid, invalid, won.</summary>
    public class MoveResult
    {
        public GameState State { get; set; }
        public string Status { get; set; }
        public string Message { get; set; }

        public MoveResult(GameState state, string status, string message)
        {
            State = state;
            Status = status;
            Message = message;
        }
    }

    public static class Puzzle
    {
        public static MoveResult NewGame()
        {
            return new MoveResult(GameState.Start(), "info",
                "The farmer and his animals are on the east bank. Pick a passenger (or none) and row across!");
        }

        public static MoveResult Move(GameState current, string passenger)
        {
            if (current == null || !current.IsValid())
                return new MoveResult(GameState.Start(), "invalid", "Invalid game state - the game has been reset.");

            passenger = string.IsNullOrWhiteSpace(passenger) ? null : passenger.Trim().ToLowerInvariant();

            if (current.IsWon)
                return new MoveResult(current, "won", "You've already won! Start a new game to play again.");

            // Is the chosen passenger on the same bank as the farmer?
            if (passenger != null)
            {
                string passengerSide = null;
                switch (passenger)
                {
                    case "fox": passengerSide = current.Fox; break;
                    case "chicken": passengerSide = current.Chicken; break;
                    case "corn": passengerSide = current.Corn; break;
                }

                if (passengerSide == null)
                    return new MoveResult(current, "invalid", "'" + passenger + "' isn't part of this puzzle.");

                if (passengerSide != current.Farmer)
                    return new MoveResult(current, "invalid",
                        "The " + passenger + " is on the other bank - the farmer can't take it.");
            }

            string target = current.Farmer == Sides.East ? Sides.West : Sides.East;

            GameState next = current.Clone();
            next.Farmer = target;
            if (passenger == "fox") next.Fox = target;
            if (passenger == "chicken") next.Chicken = target;
            if (passenger == "corn") next.Corn = target;

            // The bank the farmer just left is the only one left unsupervised.
            string unattended = current.Farmer;

            if (next.Fox == unattended && next.Chicken == unattended)
                return new MoveResult(current, "invalid", "Move rejected: the fox would eat the chicken!");

            if (next.Chicken == unattended && next.Corn == unattended)
                return new MoveResult(current, "invalid", "Move rejected: the chicken would eat the corn!");

            if (next.IsWon)
                return new MoveResult(next, "won", "Everyone made it across safely - you solved the puzzle!");

            string cargo = passenger == null ? "alone" : "with the " + passenger;
            string dir = target == Sides.West ? "west" : "east";
            return new MoveResult(next, "valid", "The farmer rowed " + cargo + " to the " + dir + " bank.");
        }
    }
}