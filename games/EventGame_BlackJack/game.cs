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

    //TODO: setup nodes for card placement
    
    // gameBrickFunction(%this,"River","setupCommunityCardsTexasHoldemDDisplay",%this);

    // for(%i = 0; %i < %this.numSeats; %i++)
    // {
    //     gameBrickFunction(%this,"Hand" @ %i,"setupPlayerTexasHoldemDisplay",%this,%i);
    // }

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
        %this.SeatFold(%seat);
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

}

function EventGame_BlackJack::startBets(%this)
{
    //after bets are done
    %this.dealCards();
}

function EventGame_BlackJack::dealCards(%this)
{
    //after cards are dealt
    %this.startPlay();
}

function EventGame_BlackJack::startPlay(%this)
{
    //after everyone has player
    %this.endPlay();
}

function EventGame_BlackJack::endPlay(%this)
{
    //after everything is revealed
    %this.doScoring();
}

function EventGame_BlackJack::doScoring(%this)
{
    //after scoring checkstart for the next game
    %this.checkStart();
}

