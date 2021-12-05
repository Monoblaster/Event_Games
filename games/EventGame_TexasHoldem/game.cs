new ScriptGroup()
{
    superClass = "EventGameType";
    class = "EventGameType_TexasHoldem";
    game = "EventGame_TexasHoldem" ;
    uiName = "Texas Holdem";
};

//$Server::EventGame::Function["EventGameType_All","NewGame"];

function EventGame_TexasHoldem::NewGame(%this,%brick,%client)
{
    %this.numSeats = %this.p0;
    %this.bigBlindValue = %this.p1;

    //# = number of players
    //playerChips# (displays the player's current chip stack)
    //playerChip# (displays dealer small blind big blind or none)
    //playerCards#(0-1) (displays both of the player's cards)
    //playerBet# (dispalys the player's current bet)
    //flop(1-5) (displays the flop)
}

function EventGame_TexasHoldem::AddPlayer(%this,%brick,%client)
{
    %thisSeat = %this.playerCount - 1;

    if(%thisSeat >= %this.numSeats)
    {
        EventGameHandler.DoCommand(%this.getgroup(),%this.name,"RemovePlayer","",%brick,%client);
    }
    else
    {
        %this.playerSeat[%this.playerCount - 1] = %thisSeat;
        %this.seatPlayer[%thisSeat] = %this.playerCount - 1;
        //once there are more than 1 players start the game
        %this.CheckStartGame();
    }    
}

//function EventGame_TexasHoldem::RemovePlayer(%this,%brick,%client){}

function EventGame_TexasHoldem::StartGame(%this)
{
    //set the ante and dealer chip
    //deal cards to everyone
    //start preflop betting
    //betting begins with the player to the left of the big blind
    %this.resetValues();
    %this.cleanTable();

    if(isObject(%this.deck))
    {
        %this.deck.delete();
    }
    %this.deck = getShuffledDeck(1);

    %this.DealPlayersCards();
    %this.SetupTable(%this.GetRandomOccupiedSeat());
    %this.startBet();
}

function EventGame_TexasHoldem::CheckStartGame(%this)
{
    %playerCount = %this.playerCount;

    if(%playerCount > 1)
    {
        %this.startGame();
    }
}

function EventGame_TexasHoldem::SetupTable(%this,%seat)
{
    %numSeats = %this.numSeats;
    for(%i = 0; %i < %numSeats; %i++)
    {
        gameBrickFunction(%this, "chip" @ %i , "disappear", -1);
    }
    %dealerChip = %seat + 0;

    %this.smallBlind = %this.getNextSeat(%dealerChip);
    %this.bigBlind = %this.getNextSeat(%this.smallBlind);

    %this.dealerChip = %dealerChip;
    %this.currTurn = %this.getNextSeat(%this.bigBlind);
    gameBrickFunction(%this, "chip" @ %dealerChip , "disappear", 0);
    gameBrickFunction(%this, "chip" @ %dealerChip , "setColor", 4);
    gameBrickFunction(%this, "chip" @ %this.smallBlind , "disappear", 0);
    gameBrickFunction(%this, "chip" @ %this.smallBlind , "setColor", 2);
    gameBrickFunction(%this, "chip" @ %this.bigBlind , "disappear", 0);
    gameBrickFunction(%this, "chip" @ %this.bigBlind , "setColor", 0);

    %this.makeRaise(%this.smallBlind,mFloor(%this.bigBlindValue / 2));
    %this.makeRaise(%this.bigBlind,mFloor(%this.bigBlindValue / 2));
}

function EventGame_TexasHoldem::cleanTable(%this)
{
    %seatCount = %this.numSeats;
    for(%i = 0; %i < %seatCount; %i++)
    {
        gameBrickFunction(%this, "hand" @ %i @ 0, "removeBrickCard");
        gameBrickFunction(%this, "hand" @ %i @ 1, "removeBrickCard");
        gameBrickFunction(%this, "bet" @ %i, "removeBrickChips");
    }

    for(%i = 0; %i < 5; %i++)
    {
        gameBrickFunction(%this, "river" @ %i, "removeBrickCard");
    }
}

function EventGame_TexasHoldem::ResetValues(%this)
{
    %this.round = 0;
    %this.playersAllIn = 0;
    %this.playersInHand = %this.playerCount;
    %this.currBet = 0;

    %seatCount = %this.numSeats;
    for(%i = 0; %i < %seatCount; %i++)
    {
        %this.seatBet[%i] = 0; 
        %this.allIn[%i] = false;
        %this.folded[%i] = false;
        %this.inHand[%i] = false;
        %this.hand[%i,0] = "";
        %this.hand[%i,1] = "";
        %this.bestHandCards[%i] = "";
        %this.bestHandEval[%i] = "";
        %this.bestHandTieNumber[%i] = "";
    }

    for(%i = 0; %i < 5; %i++)
    {
        %this.river[%i] = "";
    }
}

function EventGame_TexasHoldem::DealPlayersCards(%this)
{
    %playerCount = %this.playersInHand;
    for(%i = 0; %i < %playerCount; %i++)
    {
        %seat = %this.playerSeat[%i];
        %client = %this.player[%this.seatPlayer[%seat]];
        %player = %client.player;
        %card1 = %this.DealCard("hand" @ %seat @ 0,true);
        %card2 = %this.DealCard("hand" @ %seat @ 1,true);

        %this.hand[%seat,0] = %card1;
        %this.hand[%seat,1] = %card2;
        %player.clearCardData();
        %player.addCard(%card1);
        %player.addCard(%card2);
        %this.seatName[%seat] = %client.getPlayerName();
        %this.inHand[%seat] = true;
    }
}

function EventGame_TexasHoldem::startBet(%this)
{
    %client = %this.player[%this.seatPlayer[%this.currTurn]];
    if(%client)
    {
        %client.chatMessage("\c5The current bet is" SPC %this.currBet @ ". !raise, !call, or !fold.");
        %this.betting = true;
    }
    else
    {
        %this.seatfold(%this.currTurn);
        %this.nextbet();
    }
    
}

function EventGame_TexasHoldem::nextBet(%this)
{
    %this.currTurn = %this.getNextSeat(%this.currTurn);
    if(%this.consecutiveCalls >= %this.playersInHand || %this.playersAllIn >= %this.playersInHand || %this.playersInhand == 1)
    {
        %this.endBets();
    }
    else
    {
        %this.startBet();
    }
}

function EventGame_TexasHoldem::endBets(%this)
{
    %this.betting = false;
    switch(%this.round)
    {
        case 0:
            %this.river[0] = %this.DealCard("river" @ 0,false);
            %this.river[1] = %this.DealCard("river" @ 1,false);
            %this.river[2] = %this.DealCard("river" @ 2,false);
        case 1:
            %this.river[3] = %this.DealCard("river" @ 3,false);
        case 2:
            %this.river[4] = %this.DealCard("river" @ 4,false);
    }
    
    if(%this.round == 3 || (%this.playersAllIn >= %this.playersInHand || %this.playersInhand == 1))
    {
        %this.showdown();
    }
    else
    {
        %this.round++;
        %this.currTurn = %this.getNextSeat(%this.dealerChip);
        %this.startBet();
    }
}

function EventGame_TexasHoldem::Showdown(%this)
{
    %round = %this.round;
    if(%round == 0)
    {
        %this.river[0] = %this.DealCard("river" @ 0,false);
        %this.river[1] = %this.DealCard("river" @ 1,false);
        %this.river[2] = %this.DealCard("river" @ 2,false);
        %round++;
    }
    
    if(%round == 1)
    {
        %this.river[3] = %this.DealCard("river" @ 3,false);
        %round++;
    }

    if(%round == 2)
    {
        %this.river[4] = %this.DealCard("river" @ 4,false);
        %round++;
    }

    %seatCount = %this.numSeats;
    for(%i = 0; %i < %seatCount; %i++)
    {
        gameBrickFunction(%this, "hand" @ %i @ 0, "flipBrickCard");
        gameBrickFunction(%this, "hand" @ %i @ 1, "flipBrickCard");
    }

    %this.DetermineBestHands();
    %this.SortHands();
    %this.HandlePot();

    %this.schedule(10000,"StartGame");
}

function EventGame_TexasHoldem::serverGameCall(%this,%client)
{
    %seat = %this.playerSeat[%this.playerIndex[%client]];
    if(%this.betting && %seat == %this.currTurn)
    {
        %this.makeRaise(%seat,0);
        %this.nextBet();
    }
}

function EventGame_TexasHoldem::serverGameRaise(%this,%client)
{
    %seat = %this.playerSeat[%this.playerIndex[%client]];
    if(%this.betting && %seat == %this.currTurn)
    {
        %raiseValue = %this.p0;
        if(%raiseValue < 1)
        {
            %client.chatMessage("\c5Bet more than 0");
        }
        else
        {
            %this.makeRaise(%seat,%raiseValue);
            %this.nextBet();
        }
        
    }
}

function EventGame_TexasHoldem::ServerGameFold(%this,%client)
{
    %seat = %this.playerSeat[%this.playerIndex[%client]];
    if(%this.betting && %seat == %this.currTurn)
    {
        %this.seatfold(%seat);
        %this.nextBet();
    }
}