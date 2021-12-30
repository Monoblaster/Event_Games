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
    
    %this.gameBrickFunction("River","setupCommunityCardsTexasHoldemDDisplay",%this);

    for(%i = 0; %i < %this.numSeats; %i++)
    {
        %this.gameBrickFunction("Hand" @ %i,"setupPlayerTexasHoldemDisplay",%this,%i);
    }

    //# = number of players
    //playerChips# (displays the player's current chip stack)
    //playerChip# (displays dealer small blind big blind or none)
    //playerCards#(0-1) (displays both of the player's cards)
    //playerBet# (dispalys the player's current bet)
    //flop(1-5) (displays the flop)
}

function EventGame_TexasHoldem::EndGame(%this,%brick,%client) 
{
    
}


function EventGame_TexasHoldem::AddPlayer(%this,%brick,%client)
{
    %parameter = %this.p[0];

    if(%parameter !$= "")
    {
        %thisSeat = %parameter;
    }
    else
    {
        %thisSeat = %this.playerCount - 1;
    }
    
    if(%thisSeat >= %this.numSeats || %client.score == 0)
    {

        EventGameHandler.DoCommand(%this.getgroup(),%this.name,"RemovePlayer","",%brick,%client);
        if(%thisSeat >= %this.numSeats)
        {
            %client.centerPrint("\c6This table is full",2);
        }
        else
        {
            %client.centerPrint("\c6You have insufficient funds to play this game",2);
        }
    }
    else if(%this.getSeatPlayer(%thisSeat) !$= "")
    {
        EventGameHandler.DoCommand(%this.getgroup(),%this.name,"RemovePlayer","",%brick,%client);
        %client.centerPrint("\c6Someone is already sitting at this seat",2);
    }
    else
    {
        %this.playerSeat[%client] = %thisSeat;
        %this.seatPlayer[%thisSeat] = %client;
        //once there are more than 1 players start the game
        %this.CheckStartGame();
        %client.centerPrint("\c6Welcome to Texas Hold'em. If you do not know how to play poker look it up!",5);

        %top = vectorAdd(%this.gameBrickFunction("Seat" @ %thisSeat,"getPosition"),"0 0 " @ %brick.dataBlock.brickSizeZ/ 10);
        %client.player.setTransform(%top SPC getWords(%client.player.getTransform(),3));
    }    
}

function EventGame_TexasHoldem::RemovePlayer(%this,%brick,%client) 
{
    //remove
    %seat = %this.getPlayerSeat(%client);

    %this.playerSeat[%client] = "";
    %this.seatPlayer[%seat] = "";

    if(%this.currTurn = %seat && %this.betting)
    {
        %this.SeatFold(%seat);
    }

    %client.centerPrint("\c6You have left Texas Hold'em",2);
}


function EventGame_TexasHoldem::StartGame(%this)
{
    //set the ante and dealer chip
    //deal cards to everyone
    //start preflop betting
    //betting begins with the player to the left of the big blind
    %this.resetValues();
    %this.cleanTable();

    %this.checkSeats();

    %this.DealPlayersCards();
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
        %this.gameBrickFunction("hand" @ %i , "createBrickDealerChip", 0);
    }
    %dealerChip = %seat + 0;

    %this.smallBlind = %this.getNextSeat(%dealerChip);
    %this.bigBlind = %this.getNextSeat(%this.smallBlind);

    %this.dealerChip = %dealerChip;
    %this.currTurn = %this.getNextSeat(%this.smallBlind);
    %this.gameBrickFunction("hand" @ %dealerChip , "createBrickDealerChip", 1);
    %this.gameBrickFunction("hand" @ %this.smallBlind , "createBrickDealerChip", 2);
    %this.gameBrickFunction("hand" @ %this.bigBlind , "createBrickDealerChip", 3);

    %this.makeRaise(%this.smallBlind,mFloor(%this.bigBlindValue / 2));
    %this.makeRaise(%this.bigBlind,mFloor(%this.bigBlindValue / 2));
}

function EventGame_TexasHoldem::cleanTable(%this)
{
    %seatCount = %this.numSeats;
    for(%i = 0; %i < %seatCount; %i++)
    {
        %this.removecard("hand" @ %i,0);
        %this.removecard("hand" @ %i,1);
        %this.gameBrickFunction("hand" @ %i, "removeBrickChips");
        %this.gameBrickFunction("hand" @ %i, "createBrickDealerChip",0);
    }

    for(%i = 0; %i < 5; %i++)
    {
        %this.removeCard("river",%i);
    }
}

function EventGame_TexasHoldem::ResetValues(%this)
{
    %this.round = 0;
    %this.playersInHand = 0;
    %this.playersAllIn = 0;
    %this.currBet = 0;
    %this.betting = false;

    %seatCount = %this.numSeats;
    for(%i = 0; %i < %seatCount; %i++)
    {
        %this.seatBet[%i] = 0; 
        %this.seatAllIn[%i] = false;
        %this.seatFolded[%i] = false;
        %this.seatInHand[%i] = false;
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


function EventGame_TexasHoldem::checkSeats(%this)
{
    %numSeats = %this.numSeats;
    for(%i = 0; %i < %numSeats; %i++)
    {
        %brickPos = %this.gameBrickFunction("seat" @ %i, "getPosition");
        %mask = $TypeMasks::PlayerObjectType;
        
        %ray = containerRaycast(%brickPos,vectorAdd(%brickPos,"0 0 1"),%mask);
        %hit = getWord(%ray, 0);
        %client = %hit.client;

        if(isObject(%client) && %client.currEventGame == %this)
        {
            %seat = %this.getPlayerSeat(%client);

            //player is sitting in a different seat
            if(%seat != %i)
            {
                //remove them from their previous seat
                %this.seatPlayer[%seat] = "";

                //add them to the new seat
                %this.seatPlayer[%i] = %client;
                %this.playerSeat[%client] = %i;
            }
        }
        else if(isObject(%client) && %client.currEventGame != %this)
        {
            //center print the player to prompt game joinage
            %client.centerPrint("\c6Click a seat to join the game!",2);
        }
        else
        {
            //clear players that were sitting here
            %prev = %this.getSeatPlayer(%i);
            %this.seatPlayer[%i] = "";
            if(%prev !$= "")
            {
                %this.playerSeat[%prev] = "";
            }
        }
    }

    //remove players who no longer own seats
    %playerCount = %this.playerCount;
    for(%i = 0; %i < %playerCount; %i++)
    {
        %seat = %this.getIndexSeat(%i);

        if(%seat $= "")
        {
            %client = %this.getIndexPlayer(%i);
            EventGameHandler.DoCommand(%this.getGroup(),%this.name,"RemovePlayer","",0,%client);
            %client.centerPrint("\c6You have left the Texas Holdem table.",2);
        }
    }
}

function EventGame_TexasHoldem::DealPlayersCards(%this)
{
    if(isObject(%this.deck))
    {
        %this.deck.delete();
    }
    %this.deck = getShuffledDeck(1);

    for(%i = 0; %i < %this.playerCount; %i++)
    { 
        %seat = %this.getIndexSeat(%i);
        %client = %this.getIndexPlayer(%i);
        %player = %client.player;
        %playerChips = %client.score;
        if(%playerChips <= 0)
        {
            %this.chatMessageToPlayers("\c6" @ %client.getPlayerName() SPC "has been kicked from the table.");
            EventGameHandler.DoCommand(%this.getGroup(),%this.name,"RemovePlayer","",0,%client);
        }
        else
        {
            %card1 = %this.dealCard("hand" @ %seat,0,true);
            %card2 = %this.dealCard("hand" @ %seat,1,true);

            %this.hand[%seat,0] = %card1;
            %this.hand[%seat,1] = %card2;
            %player.clearCardData();
            %player.addCard(%card1);
            %player.addCard(%card2);
            %this.seatName[%seat] = %client.getPlayerName();
            %this.seatInHand[%seat] = true;
            %this.playersInHand++;
        }
    }
    %this.playSoundToPlayers("cardPlace" @ getRandom(1, 4) @ "Sound");

    if(%this.playersInHand > 0)
    {
        %this.SetupTable(%this.GetRandomOccupiedSeat());
        %this.endBets();
    }
}

function EventGame_TexasHoldem::startBet(%this)
{
    %client = %this.GetSeatPlayer(%this.currTurn);
    if(%client)
    {
        %client.chatMessage("\c5It is your turn. The current bet is" SPC %this.currBet @ ". !raise, !call, or !fold.");
        %this.betting = true;

        //start a schedule for to remove the player if they take too long
        %brick = %this.currbrick;
        %this.timeoutSchedule = %this.schedule(10000,"DoCommand",%this.getgroup(),%this.name,"RemovePlayer","",%brick,%client);
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
    cancel(%this.timeoutSchedule);
    if((%this.consecutiveCalls + %this.playersAllIn) >= %this.playersInHand || %this.playersInhand == 1)
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
    if(%this.round != 4)
    {
        %this.playSoundToPlayers("cardPlace" @ getRandom(1, 4) @ "Sound");
    }

    switch(%this.round)
    {
        case 1:
            %this.river[0] = %this.DealCard("river", 0,false);
            %this.river[1] = %this.DealCard("river", 1,false);
            %this.river[2] = %this.DealCard("river", 2,false);
        case 2:
            %this.river[3] = %this.DealCard("river", 3,false);
        case 3:
            %this.river[4] = %this.DealCard("river", 4,false);
    }
        
    if(%this.round == 4 || (%this.playersAllIn >= %this.playersInHand || %this.playersInhand == 1))
    {
        %this.showdown();
    }
    else
    {
        %this.consecutiveCalls = 0;
        %this.round++;
        %this.currTurn = %this.getNextSeat(%this.dealerChip);
        %this.startBet();
    }
}

function EventGame_TexasHoldem::Showdown(%this)
{
    %round = %this.round;

    if(%round != 3)
    {
        %this.playSoundToPlayers("cardPlace" @ getRandom(1, 4) @ "Sound");
    }

    if(%round == 0)
    {
        %this.river[0] = %this.DealCard("river",0,false);
        %this.river[1] = %this.DealCard("river",1,false);
        %this.river[2] = %this.DealCard("river",2,false);
        %round++;
    }
    
    if(%round == 1)
    {
        %this.river[3] = %this.DealCard("river",3,false);
        %round++;
    }

    if(%round == 2)
    {
        %this.river[4] = %this.DealCard("river",4,false);
        %round++;
    }

    %seatCount = %this.numSeats;
    for(%i = 0; %i < %seatCount; %i++)
    {
        %this.UnPeakCards(%seat);
        %this.gameBrickFunction("hand" @ %i, "flipBrickCard",0);
        %this.gameBrickFunction("hand" @ %i, "flipBrickCard",1);
    }

    %this.playSoundToPlayers("cardPick" @ getRandom(1, 4) @ "Sound");

    %this.DetermineBestHands();
    %this.SortHands();
    %this.HandlePot();

    %this.schedule(10000,"CheckStartGame");
}

function EventGame_TexasHoldem::makeRaise(%this,%seat,%value)
{
    //are you the only remaining person not all in?
    %client = %this.getSeatPlayer(%seat);

    %bet = %this.currBet;
    %playerChips = %client.score;
    %newBet = %bet + %value;
    
    
    if((%this.playersAllIn + 1) == %this.playersInHand && %this.betting && %newbet <= %playerChips)
    {
        //only 1 remaining still in hand
        %newBet = %this.currAllInBet;
        %this.chatMessageToPlayers("\c4" @ %this.seatName[%seat] SPC "\c3matches the all in\c4!");

        %pevBet = %this.seatBet[%seat];
        //refund or remove to match the previous allin
        %client.setScore(%client.score + (%prevBet - %newBet));

        %this.seatBet[%seat] = %newBet;
        %this.currBet = %newBet;
        %this.gameBrickFunction("hand" @ %seat, "createBrickChips",%newBet);
        %this.playersAllIn++;
        %this.consecutiveCalls++;
    }
    else 
    {
        if(%newbet >= %playerChips)
        {
            //all in
            if(%bet < %playerChips)
            {
                %newBet = getMin(%newBet, %playerChips);
            }
            else
            {
                %newBet = getMin(%bet, %playerChips);
            }

            if(!%this.seatAllIn[%seat])
            {
                %this.chatMessageToPlayers("\c4" @ %this.seatName[%seat] SPC "\c3goes all in\c4!");
                %this.seatAllIn[%seat] = true;
                %this.playersAllIn++;
            }
        
            %newBet += %this.seatBet[%seat];
            %this.currAllInBet = %newBet;
        }
        else if(%this.betting)
        {
             if(%value == 0)
            {
                %this.chatMessageToPlayers("\c4" @ %this.seatName[%seat] SPC "\c1calls\c4");
                %this.consecutiveCalls++;
            }
            else
            {
                %this.chatMessageToPlayers("\c4" @ %this.seatName[%seat] SPC "\c2raises\c4 by" SPC %value);
                %this.consecutiveCalls = 0;
            }
        }

        %this.currBet = %newBet;

        %client.setScore(%client.score - (%newBet - %this.seatBet[%seat]));
        %this.seatBet[%seat] = %newBet;
        %this.gameBrickFunction("hand" @ %seat, "createBrickChips",%newBet);
    }

    
}

function EventGame_TexasHoldem::SeatFold(%this,%seat)
{
    %this.chatToMessagePlayers("\c4" @ %this.seatName[%seat] SPC "\c0folds\c4!");
    %this.seatFolded[%seat] = true;
    %this.playersInHand--;
    %this.gameBrickFunction("hand" @ %seat, "removeBrickCard");
    %this.gameBrickFunction("hand" @ %seat, "removeBrickCard");
}

function EventGame_TexasHoldem::serverGameCmdCall(%this,%client)
{
    %seat = %this.getPlayerSeat(%client);
    if(%this.betting && %seat == %this.currTurn)
    {
        %this.makeRaise(%seat,0);
        %this.nextBet();
    }
}

function EventGame_TexasHoldem::serverGameCmdRaise(%this,%client)
{
    %seat = %this.getPlayerSeat(%client);
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

function EventGame_TexasHoldem::ServerGameCmdFold(%this,%client)
{
    %seat = %this.getPlayerSeat(%client);
    if(%this.betting && %seat == %this.currTurn)
    {
        %this.seatfold(%seat);
        %this.nextBet();
    }
}