function EventGame_TexasHoldem::GetNextSeat(%this,%currSeat)
{
    %seatCount = %this.numSeats;
    for(%i = 1; %i <= %seatCount; %i++)
    {
        %checkSeat = mod(%currSeat - %i,%seatCount);
        if(%this.inHand[%checkSeat] && !%this.folded[%checkSeat])
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

        if(%value == 0 && %this.betting)
        {
            chatMessagePlayers(%this,"\c4" @ %this.seatName[%seat] SPC "\c1calls\c4");
            %this.consecutiveCalls++;
        }
        else if (%this.betting)
        {
            chatMessagePlayers(%this,"\c4" @ %this.seatName[%seat] SPC "\c2raises\c4 by" SPC %value);
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
    chatMessagePlayers(%this,"\c4" @ %this.seatName[%seat] SPC "\c0folds\c4!");
    %this.folded[%seat] = true;
    %this.playersInHand--;
    gameBrickFunction(%this, "hand" @ %seat @ 0, "removeBrickCard");
    gameBrickFunction(%this, "hand" @ %seat @ 1, "removeBrickCard");
}
function EventGame_TexasHoldem::PeakCards(%this,%seat)
{
    %card0 = %this.hand[%seat,0];
    %card1 = %this.hand[%seat,1];
    %player = %this.player[%this.seatPlayer[%seat]].player;

    if(!%player.peaking)
    {
        gameBrickFunction(%this, "hand" @ %seat @ 0, "removeBrickCard");
        gameBrickFunction(%this, "hand" @ %seat @ 1, "removeBrickCard");

        %player.peaking = true;
        %player.displayCards();
        %player.isCardsVisible = 0;
        bottomprintCardInfo(%player);
    }
}

function EventGame_TexasHoldem::UnPeakCards(%this,%seat)
{
    %card0 = %this.hand[%seat,0];
    %card1 = %this.hand[%seat,1];
    %player = %this.player[%this.seatPlayer[%seat]].player;
    
    if(%player.peaking)
    {
        %player.hideCards();
        %player.peaking = false;

        gameBrickFunction(%this, "hand" @ %seat @ 0, "placeBrickCard",%card0,true);
        gameBrickFunction(%this, "hand" @ %seat @ 1, "placeBrickCard",%card1,true);
    }
}

function fxDTSBrick::placeBrickCard(%brick, %card, %down) {
    %brick.removeBrickCard();
    serverPlay3D(("cardPlace" @ getRandom(1, 4) @ "Sound"), %brick.getPosition());
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
    //disables card flipping
    %cardShape.card = "";
    //makes sure the cards know what seat they are part of (mainly for peeking)
    %name = %brick.getName();
    %cardShape.owningSeat = getSubStr(%name,strLen(%name) - 2, 1);
}

function fxDTSBrick::removeBrickCard(%brick)
{
    %card = %brick.placedCard;
    if(%card)
    {
        serverPlay3D(("cardPick" @ getRandom(1, 4) @ "Sound"), %brick.getPosition());
        %card.delete();
        %brick.placedCard = "";
    }
}

function fxDTSBrick::flipBrickCard(%brick)
{
    %card = %brick.placedCard;
    if(%card)
    {
        serverPlay3D(("cardPick" @ getRandom(1, 4) @ "Sound"), %brick.getPosition());
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

package EventGame_texasHoldem
{
    function Armor::onTrigger(%this, %obj, %trig, %val) 
    {
        %player = %obj;
		%client = %player.client;
		%s = getWords(%obj.getEyeTransform(), 0, 2);
		%masks = $TypeMasks::fxBrickObjectType | $TypeMasks::StaticObjectType | $TypeMasks::TerrainObjectType;

        %ev = %client.currEventGame;
        if  (%ev.class $= "EventGame_TexasHoldem")
        {
            %seat = %ev.playerSeat[%ev.playerIndex[%client]];
            if (%trig == 0 && %val == 1) 
            {
                if(%ev.betting)
                {
                    if (!%player.peaking) 
                    { //Flip card near hit location
                        %e = vectorAdd(vectorScale(%obj.getEyeVector(), 5), %s);
                        %ray = containerRaycast(%s, %e, %masks, %obj);
                        %hitloc = getWords(%ray, 1, 3);
                    
                        if (!isObject(getWord(%ray, 0))) {
                            return;
                        }

                        initContainerBoxSearch(%hitloc, "0.5 0.5 0.5", $TypeMasks::StaticObjectType | $TypeMasks::ItemObjectType);
                        %next = containerSearchNext();
                        if(%next.owningSeat == %seat && %next.owningSeat !$= "")
                        {
                            %ev.PeakCards(%seat);
                            serverPlay3D(("cardPick" @ getRandom(1, 4) @ "Sound"), %next.getPosition());
                        }

                        return;
                    }
                    else if (%player.peaking)
                    {

                        %ev.UnPeakCards(%seat);
                        serverPlay3D(("cardPick" @ getRandom(1, 4) @ "Sound"), %next.getPosition());

                        return;
                    }
                }
            }
        }
        parent::onTrigger(%this, %obj, %trig, %val);
    }

};
deactivatePackage("EventGame_texasHoldem");
activatePackage("EventGame_texasHoldem");