/*
 Name:		TestButtons.ino
 Created:	4/30/2021 6:01:49 PM
 Author:	civel
*/

#define BTN_PIN 3

// the setup function runs once when you press reset or power the board
void setup()
{
	pinMode(BTN_PIN, PinMode::INPUT_PULLUP);
	pinMode(LED_BUILTIN, PinMode::OUTPUT);
}

// the loop function runs over and over again until power down or reset
void loop()
{
	digitalWrite(LED_BUILTIN, digitalRead(BTN_PIN));
}
