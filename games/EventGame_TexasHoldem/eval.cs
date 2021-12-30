function EventGame_TexasHoldem::DetermineBestHands(%this)
{
    %numSeats = %this.numSeats;
    for(%i = 0; %i < %numSeats; %i++)
    {
        if(%this.seatInHand[%i] && !%this.seatFolded[%i])
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
        %insvalue = mod(%i - 1, 13);
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
            %value = %sortedHistogramValue[%i];
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
        if(%this.seatInHand[%i] && !%this.seatFolded[%i])
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

function EventGame_TexasHoldem::GetHandPrint(%this,%hand)
{
    %wordCount = getWordCount(%hand);
    for(%i = 0; %i < %wordCount; %i++)
    {
        %seperator = "";

        if(%i == (%wordCount - 2))
        {
            %seperator = " \c6and";
        }

        if(%i < (%wordCount - 1) && %wordCount > 2)
        {
            %seperator = "\c6," SPC trim(%seperator);
        }

        %cards = trim(%cards SPC getLongCardName(getWord(%hand,%i)) @ %seperator);
    }
    return %cards;
}

$Server::TexasHoldem::HandTypeName[1] = "High Card";
$Server::TexasHoldem::HandTypeName[2] = "One Pair";
$Server::TexasHoldem::HandTypeName[3] = "Two Pair";
$Server::TexasHoldem::HandTypeName[4] = "Three of a Kind";
$Server::TexasHoldem::HandTypeName[5] = "Straight";
$Server::TexasHoldem::HandTypeName[6] = "Flush";
$Server::TexasHoldem::HandTypeName[7] = "Full House";
$Server::TexasHoldem::HandTypeName[8] = "Four of a Kind";
$Server::TexasHoldem::HandTypeName[9] = "Straight Flush";
function EventGame_TexasHoldem::HandlePot(%this)
{
    %seatCount = %this.numSeats;
    %sortedCount = %this.sortedCount;
    %firstPot = true;
    for(%i = 0; %i < %sortedCount; %i++)
    {
        %seat = %this.sorted[%i];
        %bet = %this.seatBet[%seat];
        %client = %this.getSeatPlayer(%seat);
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
            %pot = "a side";
            if(%firstPot)
            {
                %pot = "the main";
            }
            %client.setScore(%client.score + %totalGain);
            %this.chatMessageToPlayers("\c3" @ %this.seatName[%seat] SPC "wins" SPC %pot SPC "pot of" SPC %totalGain SPC "chips with a" SPC $Server::TexasHoldem::HandTypeName[getWord(%this.bestHand[%seat],0)] @ "!");
            %this.chatMessageToPlayers(%this.GetHandPrint(getWords(%this.bestHand[%seat],1)));
            %firstPot = false;
        }
        
    }
    
}