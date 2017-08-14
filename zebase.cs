   ///////////////////////////////
  // Fast Database Script V2.0 //
 // Created by Zeblote (1163) //
///////////////////////////////

function Zebase::debug(%this, %message){if(%this.debug)echo(%this.getname() SPC %message);}
function Zebase::registerValue(%this, %valuename, %default) {
	if(%valuename $= "" || %this.hasValue[%valuename])return;
	%this.debug("registering value " @ %valuename @ " to ID " @ %this.valuecount @ " with default " @ %default);
	%this.valueName[%this.valuecount] = %valuename;
	%this.defaultValue[%this.valuecount] = %default;
	%this.valuecount++;
	%this.hasValue[%valuename] = 1;
	for(%i = 0; %i < %this.getcount(); %i++)
	{
		%obj = %this.getobject(%i);
		%obj.value[%valuename] = %default;
	}
	%this.dataSheet.value[%valuename] = %default;
}
function Zebase::createData(%this, %bl_id) {
	if(isObject(%this.Data[%bl_id]))return;	
	%this.debug("creating data " @ %bl_id @ " with ID " @ %this.dataCount);
	%this.datasheet.setname("ZedataSheet");
	%this.data[%bl_id] = new scriptObject(Temp:ZedataSheet){};
	%this.datasheet.setname("");
	%this.data[%bl_id].setname("Zedata_" @ %this.getname() @ "_" @ %bl_id);
	%this.data[%bl_id].value[bl_id] = %bl_id;
	%this.add(%this.data[%bl_id]);
	%this.data[%bl_id].num = %this.dataCount;
	%this.numData[%this.dataCount] = %bl_id;
	%this.dataCount++;
}
function Zebase::save(%this, %path) {
	%this.debug("saving data to " @ %path);
	filedelete(%path);
	parent::save(%this,%path);
}
function Zebase::resetData(%this, %bl_id) {
	if(!isObject(%this.Data[%bl_id]))return;
	%this.debug("resetting data " @ %bl_id);
	%num = %this.data[%bl_id].num;
	%this.data[%bl_id].delete();
	%this.datasheet.setname("ZedataSheet");
	%this.data[%bl_id] = new scriptObject(Temp:ZedataSheet){};
	%this.datasheet.setname("");
	%this.data[%bl_id].setname("Zedata_" @ %this.getname() @ "_" @ %bl_id);
	%this.data[%bl_id].value[bl_id] = %bl_id;
	%this.data[%bl_id].num = %num;
}
function Zebase::createDataSheet(%this) {
	%this.debug("generating dataSheet with " @ %this.valueCount @ " values");
	if(isObject(%this.dataSheet))
		if(%this.datasheet.getname() $= "ZedataSheet")
			%this.dataSheet.delete();
	%this.dataSheet = new ScriptObject(){};
	%i = -1;
	while(%this.valueName[%i++] !$= "")
		eval(%this.dataSheet @ ".value[\"" @ %this.valueName[%i] @ "\"] = \"" @ %this.defaultValue[%i] @ "\";");
}
function Zebase::getValue(%this, %bl_id, %valueName) {
	if(!isObject(%this.data[%bl_id]) || !%this.hasValue[%valuename])return;
	%this.debug("getting value " @ %valuename @ " of " @ %bl_id @ ", return " @ %this.data[%bl_id].value[%valuename]);
	return %this.data[%bl_id].value[%valuename];
}
function Zebase::setValue(%this, %bl_id, %valueName, %value) {
	if(!isObject(%this.data[%bl_id]) || !%this.hasValue[%valuename])return;
	%this.debug("setting value " @ %valuename @ " of " @ %bl_id @ " to " @ %value);
	%this.data[%bl_id].value[%valuename] = %value;
}
function Zebase::incValue(%this, %bl_id, %valueName, %increase) {
	if(!isObject(%this.data[%bl_id]) || !%this.hasValue[%valuename])return;
	%this.debug("increasing value " @ %valuename @ " of " @ %bl_id @ " by " @ %increase);
	%this.data[%bl_id].value[%valuename] = %this.getvalue(%bl_id, %valuename) + %increase;
}

function Zebase::sortlist(%this) {
	%this.debug("sorting data list");
	%count = %this.getcount();
	echo("COUNT: " @ %count);
	%this.datacount = 0;
	for(%chalmanint = 0; %chalmanint < %count; %chalmanint++) {
		echo("  NEW SET: " @ %chalmanint);
		%bl_id = %this.getobject(%chalmanint).value["bl_id"];
		echo("    ID: " @ %bl_id);
		%this.data[%bl_id] = %this.getObject(%chalmanint).getid();
		echo("    OBJID: " @ %this.getObject(%chalmanint).getid());
		%this.numdata[%this.datacount] = %bl_id;
		%this.getobject(%chalmanint).num = %this.datacount;
		%this.datacount++;
		if(%this.debug)
			for(%asdf = 0; %asdf < %this.valueCount; %asdf++)
				echo("    VALUE " @ %this.valuename[%asdf] @ ": " @ %this.getObject(%chalmanint).value[%this.valuename[%asdf]]);
	}
}