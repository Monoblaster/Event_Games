function EventGame_BlackJack::getNextSeat(%start)
{
    //TODO: return the next availible seat from the starting seat
}

function fxDTSBrick::setupBlackJackNodes(%brick)
{
    //TODO: generates nodes based on 0 0 0 and facing with this normal "1 0 0"
}


function EventGame_BlackJack::GetRandomOccupiedSeat(%this)
{
    %playerCount = %this.playerCount;
    
    return %this.playerSeat[getRandom(0,%this.playerCount - 1)];
}



function EventGame_BlackJack::DealCard(%this,%brickName,%slot,%down,%delay)
{
    %currCard = gameBrickFunction(%this,%brickName,"getBrickCard",%slot);
    if(%currCard $= "")
    {
        %card = %this.deck.removeCard();

        if(!%delay)
        {
            %this.gameBrickFunction(%this,%brickName,"PlaceBRickCard",%slot,%card,%down);
        }
        else
        {
            schedule(%delay,%this,"gameBrickFunction",%this, %brickname, "placeBrickCard",%slot,%card,%down);
        }
        return %card;
    }
    return "";
}

function EventGame_BlackJack::RemoveCard(%this,%brickName,%slot,%delay)
{
    %card = gameBrickFunction(%this,%brickName,"getBrickCard",%slot);
    if(%card !$= "")
    {
        if(!%delay)
        {
            %this.gameBrickFunction(%this,%brickName,"removeBrickCard",%slot);
        }
        else
        {
            schedule(%delay,%this,"gameBrickFunction",%this, %brickname, "removeBrickCard",%slot);
        }
    }
}

function fxDTSBrick::getBrickCard(%brick,%slot)
{
    return %brick.placedCard[%slot];
}

function fxDTSBrick::placeBrickCard(%brick, %slot, %card, %down) 
{
    %brick.removeBrickCard(%slot);
    %brickName = %brick.getName();
    serverPlay3D(("cardPlace" @ getRandom(1, 4) @ "Sound"), %brick.getPosition());
	
	%cardShape = new StaticShape(CardShapes) {
		dataBlock = CardShape;
		card = %card;
	};
    %brick.placedCard[%slot] = %cardShape;

    %handStrPos = striPos(%brickName,"hand");
    if(%handStrPos != -1)
    {
        %cardShape.owningSeat = getSubStr(%brickName, %handStrPos + 4, 1);
    }

    %cardShape.brick = %brick;
	%cardShape.setTransform(%brick.gameGetNode("card" @ %slot));

	if (!%down) {
		%cardShape.playThread(0, cardFaceUp);
	} else {
		%cardShape.playThread(0, cardFaceDown);
	}

	%cardShape.down[%slot] = %down;

	cardDisplay(%cardShape, getCardName(%card));
    //disables card flipping
    %cardShape.card = "";
}

function fxDTSBrick::removeBrickCard(%brick,%slot)
{
    %card = %brick.placedCard[%slot];
    if(%card)
    {
        serverPlay3D(("cardPick" @ getRandom(1, 4) @ "Sound"), %brick.getPosition());
        %brick.placedCard[%slot] = "";
        %card.delete();
    }
}

function fxDTSBrick::flipBrickCard(%brick,%slot)
{
    %card = %brick.placedCard[%slot];
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
	%loc = %b.gameGetNode("bet");

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