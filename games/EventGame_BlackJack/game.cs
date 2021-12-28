new ScriptGroup()
{
    superClass = "EventGameType";
    class = "EventGameType_BlackJack";
    game = "EventGame_BlackJack";
    uiName = "Black Jack";
};

//$Server::EventGame::Function["EventGameType_All","NewGame"];

function EventGame_BlackJack::NewGame(%this,%brick,%client)
{
    %this.numSeats = %this.p0;

    //all players use the nodes stored on the dealer brick
    //TODO: maybe not?
    gameBrickFunction(%this,"Dealer","setupBlackJackNodes",%this);
}

function EventGame_BlackJack::EndGame(%this,%brick,%client) 
{
    
}


function EventGame_BlackJack::AddPlayer(%this,%brick,%client)
{
    %parameter = %this.p[0];

    if(%parameter !$= "")
    {
        %thisSeat = %parameters;
    }
    else
    {
        %thisSeat = %this.playerCount - 1;
    }
    
    if(%thisSeat >= %this.numSeats || %client.score == 0)
    {
        if(%thisSeat >= %this.numSeats)
        {
            %client.centerPrint("\c6This table is full",2);
        }
        else
        {
            %client.centerPrint("\c6You have insufficient funds to play this game",2);
        }
        EventGameHandler.DoCommand(%this.getgroup(),%this.name,"RemovePlayer","",%brick,%client);
    }
    else if(%this.seatPlayer[%thisSeat] !$= "")
    {
        %client.centerPrint("\c6Someone is already sitting at this seat",2);
    }
    else 
    {
        %this.playerSeat[%this.playerCount - 1] = %thisSeat;
        %this.seatPlayer[%thisSeat] = %this.playerCount - 1;
        //once there are more than 1 players start the game
        %this.CheckStartGame();
        %client.centerPrint("\c6Welcome to Black Jack. If you do not know how to play Black Jack look it up!",5);
    }    
}

function EventGame_BlackJack::RemovePlayer(%this,%brick,%client) 
{
    //remove
    %seat = %this.playerSeat[%this.playerIndex[%client]];

    if(%this.currTurn = %seat)
    {
        //TODO: make them instantly bust if they leave the game
    }

    %client.centerPrint("\c6You have left Black Jack",2);
}


function EventGame_BlackJack::StartGame(%this)
{
    //clear cards
    %this.cleanTable();
    //get bets from every player
    //deal cards to players face up
    //leave one of the dealer's cards face up
    //safety
    //start with the counter clockwise most player
    //if they can split let them split
    //resolve the players cards by letting them hit until they stay or bust
    //after everyone has stayed or busted reveal the facedown player card
    //it is now the dealers turn to play
    //after the dealer is done score everyone by who lost/tied/beat the dealer
    %this.startBets();
    
}

function EventGame_BlackJack::CheckStartGame(%this)
{
    %playerCount = %this.playerCount;

    if(%playerCount >= 1)
    {
        %this.startGame();
    }
}

function EventGame_BlackJack::cleanTable(%this)
{
    //TODO: remove all player cards and chips from the table
}

function EventGame_BlackJack::startBets(%this)
{
    //start betting
    //betting is asynchronous so there is a 10 second timer
    //prompt all players who are in the game
    %this.betting = true;
    chatMessagePlayers(%this,"\c3Time to bet! Use \c4!bet \c3to make your bet.");

    //ten second timeup
    %this.schedule("10000","endBets");
}

function EventGame_BlackJack::endBets(%this)
{
    %this.betting = false;
    //after bets are done
    %this.dealCards();
}

function EventGame_BlackJack::dealCards(%this)
{
    %playerCount = %this.palyerCount;
    for(%i = 0; %i < %playerCount; %i++)
    {
        %seat = %this.PlayerSeat[%i];
        %this.dealCard("player" @ %seat, 0, false);
    }

    %this.dealCard("dealer", 0,false);

    for(%i = 0; %i < %playerCount; %i++)
    {
        %seat = %this.PlayerSeat[%i];
        %this.dealCard("player" @ %seat, 1);
    }

    %this.dealCard("dealer", 1,false);

    //after cards are dealt
    %this.schedule(200 * (%i + %playerCount + 1),"startPlay");
}

function EventGame_BlackJack::startPlay(%this)
{
    %this.currTurn = -1;
    %this.turnsCompleted = 0;
    %this.playing = true;
    %this.nextTurn();
}

function EventGame_BlackJack::nextTurn(%this)
{
    if(%this.turnsCompleted >= %this.playerCount)
    {
        %this.currTurn = %this.getNextSeat(%this.currTurn + 1);
        %client = %this.player[%this.seatPlayer[%this.currTurn]];
        %client.chatMessage("\c3It is your turn to play. !hit, !stand, or !doubledown to finish playing your hand.");
        //TODO: check if the player can split and prompt accodingly
    }
    else
    {
        //after everyone has played
        %this.endPlay();
    }
    %this.turnsCompleted++;
}

function EventGame_BlackJack::checkEndTurn(%this)
{
    %currTurn = %this.currTurn;

    if(%this.stand[%currTurn] || %this.bust[%currTurn] || %this.doubleDown[%currTurn])
    {
        %this.nextTurn();
    }
}

function EventGame_BlackJack::endPlay(%this)
{
    %this.playing = false;
    //after everything is revealed
    %this.doScoring();
}

function EventGame_BlackJack::doScoring(%this)
{
    //after scoring checkstart for the next game
    %this.checkStart();
}

function EventGame_BlackJack::serverGameBet(%this,%client)
{
    //TODO: add bet
}

function EventGame_BlackJack::serverGameHit(%this,%client)
{
    %seat = %this.playerSeat[%this.playerIndex[%client]];
    if(%this.playing && %seat == %this.currTurn)
    {
        %this.hand[%seat,%this.hitCount[%seat] + 2] = %this.dealCard("player" @ %seat,%this.hitCount[%seat] + 2,false);
        //TODO: check for bust
    }
}

function EventGame_BlackJack::serverGameStand(%this,%client)
{
    //TODO: add Stand
}

function EventGame_BlackJack::serverGameDoubleDown(%this,%client)
{
    //TODO: add DoubleDown
}

function EventGame_BlackJack::serverGameSplit(%this,%client)
{
    //TODO: add Split
}

