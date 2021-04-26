/*
 Name:		TestArduinoUDP.ino
 Created:	4/23/2021 6:34:16 PM
 Author:	civel
*/

#include <RTCZero.h>
#include <ArduinoLowPower.h>
#include <WiFiNINA.h>
#include <WiFiUdp.h>
#include "src/Definitions.h"
#include "Serializable.h"
#include "Serializer.h"
#include "WiFiUDPSerializer.h"

WiFiUDP udp;
IPAddress address(192, 168, 2, 11);




void Send()
{
	
}

// the setup function runs once when you press reset or power the board
void setup()
{
	pinMode(0, INPUT);
#ifdef DEBUG
	Serial.begin(115200);
#endif
	DebugLine("Connecting");
	WiFi.begin(WIFI_SSID, WIFI_PASSWORD);
	DebugLine("Connected");
	uint8_t error = udp.begin(7000);
	DebugAssertEqual(error, 1);
	IPAddress address = WiFi.localIP();
	DebugValue(address);
	//LowPower.attachInterruptWakeup(0, Send, CHANGE);
	WiFi.lowPowerMode();
	
}

// the loop function runs over and over again until power down or reset
void loop()
{
	udp.beginPacket(address, 7000);
	udp.println(digitalRead(0));
	udp.endPacket();
}
