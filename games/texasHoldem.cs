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
    %this.playerSeat[%this.playerCount - 1] = %thisSeat;
    %this.seatPlayer[%thisSeat] = %this.playerCount - 1;
    //once there are more than 1 players start the game
    %this.CheckStartGame();
}

function EventGame_TexasHoldem::RemovePlayer(%this,%brick,%client)
{
    //TODO: check if the game should continue and take this player out of the hand as if they folded
}

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
        %card1 = %this.DealCard("hand" @ %seat @ 0,true);
        %card2 = %this.DealCard("hand" @ %seat @ 1,true);

        %client.chatMessage(getLongCardName(%card1) SPC "and a" SPC getLongCardName(%card2));
        %this.hand[%seat,0] = %card1;
        %this.hand[%seat,1] = %card2;

        %this.inHand[%seat] = true;
    }
}

function EventGame_TexasHoldem::startBet(%this)
{
    %client = %this.player[%this.seatPlayer[%this.currTurn]];
    %client.chatMessage("The current bet is" SPC %this.currBet @ ". !raise, !call, or !fold.");
    %this.betting = true;
}

function EventGame_TexasHoldem::nextBet(%this)
{
    %this.currTurn = %this.getNextSeat(%this.currTurn);
    if(%this.consecutiveCalls >= %this.playersInHand || %this.playersAllIn >= %this.playersInHand)
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
    
    if(%this.round == 3)
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

function EventGame_TexasHoldem::DetermineBestHands(%this)
{
    %numSeats = %this.numSeats;
    for(%i = 0; %i < %numSeats; %i++)
    {
        if(%this.inHand[%i] && !%this.folded[%i])
        {
            $Server::TexasHoldem::BestHand = "";
            %this.bestHand[%i] = %this.getBestHand(%i);
        }
    }
}
$c = -1;
$Server::TexasHoldem::PossibleHands[$c++] = "34567";
$Server::TexasHoldem::PossibleHands[$c++] = "24567";
$Server::TexasHoldem::PossibleHands[$c++] = "23567";
$Server::TexasHoldem::PossibleHands[$c++] = "23467";
$Server::TexasHoldem::PossibleHands[$c++] = "23457";
$Server::TexasHoldem::PossibleHands[$c++] = "23456";
$Server::TexasHoldem::PossibleHands[$c++] = "14567";
$Server::TexasHoldem::PossibleHands[$c++] = "13567";
$Server::TexasHoldem::PossibleHands[$c++] = "13467";
$Server::TexasHoldem::PossibleHands[$c++] = "13457";
$Server::TexasHoldem::PossibleHands[$c++] = "13456";
$Server::TexasHoldem::PossibleHands[$c++] = "12567";
$Server::TexasHoldem::PossibleHands[$c++] = "12467";
$Server::TexasHoldem::PossibleHands[$c++] = "12457";
$Server::TexasHoldem::PossibleHands[$c++] = "12456";
$Server::TexasHoldem::PossibleHands[$c++] = "12367";
$Server::TexasHoldem::PossibleHands[$c++] = "12357";
$Server::TexasHoldem::PossibleHands[$c++] = "12356";
$Server::TexasHoldem::PossibleHands[$c++] = "12347";
$Server::TexasHoldem::PossibleHands[$c++] = "12346";
$Server::TexasHoldem::PossibleHands[$c++] = "12345";
function EventGame_TexasHoldem::getBestHand(%this,%seat)
{
    %card[1] = %this.river[0];
    %card[2] = %this.river[1];
    %card[3] = %this.river[2];
    %card[4] = %this.river[3];
    %card[5] = %this.river[4];
    %card[6] = %this.hand[%seat,0];
    %card[7] = %this.hand[%seat,1];
    %bestEvalHand = "";

    for(%i = 0; %i < 21; %i++)
    {
        %handCombo = $Server::TexasHoldem::PossibleHands[%i];
        %hand = "";
        for(%j = 0; %j < 5 ; %j++)
        {
            %card = getSubStr(%handCombo,%j,1);
            %hand = trim(%hand SPC %card[%card]); 
        }

        %eval = %this.HandType(%hand);
        if(%this.IsHandABetter(%eval,%bestEvalHand ))
        {
            %bestEvalHand  = %eval;
        }
    }
    
    return %bestEvalHand;
}
//eval is just the handType
//kickers are evaluated elswhere depending on the hand type
$Server::TexasHoldem::HandType["High Card"] = 1;
$Server::TexasHoldem::HandType["One Pair"] = 2;
$Server::TexasHoldem::HandType["Two Pair"] = 3;
$Server::TexasHoldem::HandType["Three of a Kind"] = 4;
$Server::TexasHoldem::HandType["Straight"] = 5;
$Server::TexasHoldem::HandType["Flush"] = 6;
$Server::TexasHoldem::HandType["Full House"] = 7;
$Server::TexasHoldem::HandType["Four of a Kind"] = 8;
$Server::TexasHoldem::HandType["Straight Flush"] = 9;
function EventGame_TexasHoldem::HandType(%this,%hand)
{
    %type = $Server::TexasHoldem::HandType["High Card"];
    //create histogram
    %sameSuit = (1 << mFloor(getWord(%hand,0) / 13));
    for(%i = 0; %i < 5; %i++)
    {
        %rank = getWord(%hand,%i) % 13;
        %histogram[%rank]++;
        %sameSuit = %sameSuit & (1 << mFloor(getWord(%hand,%i) / 13));
    }
    //sort the histogram for counting
    %sortedCount = 0;
    %numUniqueRanks = 0;
    for (%i = 0; %i < 13; %i++)
    {
        %value = %histogram[%i];

        if(%value > 0)
        {
            %numUniqueRanks++;
        }

        for(%j = 0; %j < %sortedCount; %j++)
        {
            %valueS = %sortedHistogram[%j];
            %rankS = %sortedHistogramValue[%j];
            if(%value > %valueS ||  (%i > %rankS && %value == %valueS))
            {
                break;
            }
        }

        %sortedCount++;
        %temp = "";
        %ins = %value;
        %insvalue = %i;
        for(%k = %j; %k < %sortedCount; %k++)
        {
            %temp = %sortedHistogram[%k];
            %sortedHistogram[%k] = %ins;
            %ins = %temp;
            %temp = %sortedHistogramValue[%k];
            %sortedHistogramValue[%k] = %insValue;
            %insValue = %temp;
        }
    }
    //check histogram values
    %checkValue = (%sortedHistogram[0] * 100) + (%sortedHistogram[1] * 10) + (%sortedHistogram[2]);
    switch(%checkValue)
    {
        case 410:
            %type = $Server::TexasHoldem::HandType["Four of a Kind"];
        case 320:
            %type = $Server::TexasHoldem::HandType["Full House"];
        case 311:
            %type = $Server::TexasHoldem::HandType["Three of a Kind"];
        case 221:
            %type = $Server::TexasHoldem::HandType["Two Pair"];
        default:
            if(%numUniqueRanks == 4)
            {
                %type = $Server::TexasHoldem::HandType["One Pair"];
            }
    }
    //check for a flush
    if(%sameSuit)
    {
        %type = $Server::TexasHoldem::HandType["Flush"];
    }
    %sortedCount = 0;
    for (%i = 0; %i < 5; %i++)
    {
        %card = getWord(%hand,%i);
        %value = mod((%card - 1), 13);

        for(%j = 0; %j < %sortedCount; %j++)
        {
            %valueS = %sortedValues[%j];
            if(%value > %valueS)
            {
                break;
            }
        }

        %sortedCount++;
        %temp = "";
        %insVal = %value;
        %insCar = %card;
        for(%k = %j; %k < %sortedCount; %k++)
        {
            %temp = %sortedValues[%k];
            %sortedValues[%k] = %insVal;
            %insVal = %temp;
            %temp = %sortedCards[%k];
            %sortedCards[%k] = %insCar;
            %insCar = %temp;
        }
    }

    if((%sortedValues[4] - %sortedValues[0] == 4))
    {
        if(%type == $Server::TexasHoldem::HandType["Flush"])
        {
            %type = $Server::TexasHoldem::HandType["Straight Flush"];
        }
    }

    if(%type == 2 || %type == 3 || %type == 4 || %type == 8)
    {
        //sort cards with the sorted histogram in mind
        for(%i = 0; %i < 4; %i++)
        {
            %value = mod(%sortedHistogramValue[%i] - 1,13);
            %ammount = %sortedHistogram[%i] + 0;
            for(%j = 0; %j < 5; %j++)
            {
                %rank = %sortedValues[%j];
                %cardValue = %sortedCards[%j];
                if((%ammount == 1 && !%use[%cardValue]) || (%rank == %value && %ammount > 1))
                {
                    %handYs = trim(%handYs SPC %cardValue);
                    %use[%cardValue] = true;
                }
            }
        }
    }
    else
    {
        for(%i = 0; %i < 5; %i++)
        {
            %handYs = trim(%handYs SPC %sortedCards[%i]);
        }
    }
    return %type SPC %handYs;
}
//returns if evalled a is better than evalled b
function EventGame_TexasHoldem::IsHandABetter(%this,%a,%b)
{
    %typeA = getWord(%a,0);
    %typeB = getWord(%b,0);
    if(%typeA != %typeB)
    {
        return %typeA > %typeB;
    }
    else
    {
        //compare each card until one is lower
        for(%i = 1; %i < 6; %i++)
        {
            %carda = mod(getWord(%a,%i) - 1,13);
            %cardb = mod(getWord(%b,%i) - 1,13);
            if(%cardA != %cardB)
            {
                return %carda > %cardb;
            }
        }
    }
}

function EventGame_TexasHoldem::SortHands(%this)
{
    %numSeats = %this.numSeats;
    %sortedCount = 0;
    for (%i = 0; %i < %numSeats; %i++)
    {
        if(%this.inHand[%i] && !%this.folded[%i])
        {
            %value = %this.bestHand[%i];

            for(%j = 0; %j < %sortedCount; %j++)
            {
                %valueS = %sorted[%j];
                if(%this.IsHandABetter(%value,%valueS))
                {
                    break;
                }
            }

            %sortedCount++;
            %temp = "";
            %ins = %value;
            %insSeat = %i;
            for(%k = %j; %k < %sortedCount; %k++)
            {
                %temp = %sorted[%k];
                %sortedHistogram[%k] = %ins;
                %ins = %temp;
                %temp = %sortedSeat[%k];
                %sortedSeat[%k] = %insSeat;
                %insSeat = %temp;
            }
        }   
    }

    %this.sortedCount = %sortedCount;
    for(%i = 0; %i < %numseats; %i++)
    {
        %this.sorted[%i] = %sortedSeat[%i];
    }
}

function EventGame_TexasHoldem::PrintHand(%this,%hand)
{
    for(%i = 0; %i < 5; %i++)
    {
        %cards = trim(%cards SPC getLongCardName(getWord(%hand,%i)));
    }
    talk(%cards);
}

function EventGame_TexasHoldem::HandlePot(%this)
{
    %seatCount = %this.numSeats;
    %sortedCount = %this.sortedCount;
    for(%i = 0; %i < %sortedCount; %i++)
    {
        %seat = %this.sorted[%i];
        %bet = %this.seatBet[%seat];
        %client = %this.player[%this.seatPlayer[%seat]];
        %totalGain = 0;
        //substract up to the bet from the pot
        if(%bet > 0)
        {
            for(%j = 0; %j < %seatCount; %j++)
            {
                %sBet = %this.seatBet[%j];
                %subtract = getMin(%sBet, %bet);
                %this.seatBet[%j] = %sbet - %subtract;
                %totalGain += %subtract;
            }
        }

        if(%totalGain > 0)
        {
            %client.setScore(%client.score + %totalGain);
            %this.printHand(getWords(%this.bestHand[%i],1));
        }
    }
    
}

function EventGame_TexasHoldem::GetNextSeat(%this,%currSeat)
{
    %seatCount = %this.numSeats;
    for(%i = 1; %i <= %seatCount; %i++)
    {
        %checkSeat = mod(%currSeat - %i,%seatCount);
        if(%this.inHand[%checkSeat])
        {
            return %checkSeat;
        }
    }

    return -1;
}

function EventGame_TexasHoldem::GetRandomOccupiedSeat(%this)
{
    %playerCount = %this.playerCount;
    
    return %this.playerSeat[getRandom(0,%this.playerCount - 1)];
}

function EventGame_TexasHoldem::makeRaise(%this,%seat,%value)
{
    //are you the only remaining person not all in?
    %client = %this.player[%this.seatPlayer[%seat]];
    if((%this.playersAllIn + 1) == %this.playersInHand && !%this.AllIn[%seat])
    {
        %newBet = %this.currBet;

        if(%newBet >= %client.score)
        {
            %this.allIn[%seat] = true;
            %newBet = %client.score;
        }

        %this.playersAllIn++;
    }
    else
    {
        %bet = %this.currBet;
        %newBet = %bet + %value;

        if(%newBet >= %client.score)
        {
            %newBet = %client.score;
            %this.allIn[%seat] = true;
            %this.playersAllIn++;
        }

        if(%value == 0)
        {
            %this.consecutiveCalls++;
        }
        else
        {
            %this.consecutiveCalls = 0;
        }
    }
    
    %client.setScore(%client.score - (%newBet - %this.seatBet[%seat]));
    %this.seatBet[%seat] = %newBet;
    gameBrickFunction(%this, "bet" @ %seat, "createBrickChips",%newBet);

    %this.currBet = %newBet;
}

function EventGame_TexasHoldem::DealCard(%this,%brickName,%down)
{
    %card = %this.deck.removeCard();
    gameBrickFunction(%this, %brickname, "placeBrickCard",%card,%down);
    return %card;
}

function EventGame_TexasHoldem::SeatFold(%this,%seat)
{
    %this.folded[%seat] = true;
    %this.playersInHand--;
    gameBrickFunction(%this, "hand" @ %seat @ 0, "removeBrickCard");
    gameBrickFunction(%this, "hand" @ %seat @ 1, "removeBrickCard");
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
            %client.chatMessage("Bet more than 0");
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

function fxDTSBrick::placeBrickCard(%brick, %card, %down) {
    %brick.removeBrickCard();
    %dir = %brick.itemDirection;
	if (%dir == 2)
	{
		%rot = "0 0 1 0";
	}
	else if (%dir == 3)
	{
		%rot = "0 0 1 " @ $piOver2;
	}
	else if (%dir == 4)
	{
		%rot = "0 0 -1 " @ $pi;
	}
	else if (%dir == 5)
	{
		%rot = "0 0 -1 " @ $piOver2;
	}
	else 
	{
		%rot = "0 0 1 0";
	}

    %pos = vectorAdd(%brick.getPosition(),"0 0 " @ %brick.dataBlock.brickSizeZ/ 10);
	
	%cardShape = new StaticShape(CardShapes) {
		dataBlock = CardShape;
		card = %card;
	};
    %brick.placedCard = %cardShape;
	%cardShape.setTransform(%pos SPC %rot);
	if (!%down) {
		%cardShape.playThread(0, cardFaceUp);
	} else {
		%cardShape.playThread(0, cardFaceDown);
	}

	%cardShape.down = %down;

	cardDisplay(%cardShape, getCardName(%card));
}

function fxDTSBrick::removeBrickCard(%brick)
{
    %card = %brick.placedCard;
    if(%card)
    {
        %card.delete();
        %brick.placedCard = "";
    }
}

function fxDTSBrick::flipBrickCard(%brick)
{
    %card = %brick.placedCard;
    if(%card)
    {
        %down = %card.down;
        if (%down) {
            %card.playThread(0, cardFaceUp);
        } else {
            %card.playThread(0, cardFaceDown);
        }
    }
}

function fxDTSBrick::createBrickChips(%b,%value)
{
	if (%value <= 0) {
		%b.removeBrickChips();
		return;
	}
	%loc = vectorAdd(%b.getPosition(), "0 0 " @ %b.getDatablock().brickSizeZ * 0.1);

	%chipVector = getChipCounts(%value);
	%count = 0;
	%largestChipCount = 0;
	for (%i = 0; %i < getWordCount(%chipVector); %i++) {
		%chipCount = getWord(%chipVector, %i);
		if (!isObject(%b.chip[%count]) && %chipCount != 0) {
			%b.chip[%count] = new StaticShape(ChipDisplayShapes) {
				datablock = ChipShape;
			};
		} else if (isObject(%b.chip[%count]) && %chipCount == 0) {
			%b.chip[%count].delete();
		}

		if (!isObject(%b.chip[%count])) {
			continue;
		}
		%b.chip[%count].setNodeColor("ALL", $ChipType[%i]);
		%b.chip[%count].setScale("1 1 " @ %chipCount);
		// %b.chip[%count].chipValue = %chipCount * $ChipType[%i @ "Cost"];

		// %b.chip[%count].setShapeNameColor(getWords($ChipType[%i], 0, 2));
		// %b.chip[%count].setShapeName(%chipCount @ " x " @ $ChipType[%i @ "Cost"]);

		if (%chipCount > %largestChipCount) {
			%largestChipCount = %chipCount;
			%largest = %count;
		}
		%b.chip[%count].setShapeName("");
		%b.chip[%count].setShapeNameColor("1 1 1");

		%b.chip[%count].setTransform(vectorAdd($offset[%count], %loc));
		%count++;
	}

	for (%i = %count; %i < getWordCount(%chipVector); %i++) {
		if (isObject(%b.chip[%i])) {
			%b.chip[%i].delete();
		}
	}

	if (isObject(%b.chip[%largest])) {
		%b.chip[%largest].setShapeName(%value);
	}

	%b.isDisplayingChips = 1;
}

function fxDTSBrick::removeBrickChips(%b) {
	for (%i = 0; %i < 10; %i++) {
		if (isObject(%b.chip[%i])) {
			%b.chip[%i].delete();
		}
	}

	%b.isDisplayingChips = 0;
}

//TODO: allow players to pickup and view their cards instead of having them recieved in chat
//TODO: finish player interaction: messages when someone raises or folds, sound effects, and final scoring announcements 