// Packet.h

#ifndef _PACKET_h
#define _PACKET_h

#if defined(ARDUINO) && ARDUINO >= 100
	#include "arduino.h"
#else
	#include "WProgram.h"
#endif

#include "RGBled.h"
#include "Print.h"



enum RemoteType : uint8_t
{
	LeftRemote = (uint8_t)1,
	RightRemote = (uint8_t)2,
};

struct Button
{
	uint8_t pin;
	bool invert;
	bool consolidate;
	bool lastVal;
	uint8_t solid;
	uint8_t consolidateCount;

	Button(uint8_t p, bool inv)
	{
		pin = p;
		invert = inv;
	}

	Button(uint8_t p, bool inv, uint8_t consCount)
	{
		pin = p;
		invert = inv;
		consolidate = true;
		consolidateCount = consCount;
		solid = 0;
	}

	void Init()
	{
		pinMode(pin, INPUT_PULLUP);
	}

	uint8_t GetState()
	{
		if (!consolidate) return digitalRead(pin) != invert;

		if (digitalRead(pin) != invert) solid++;
		else solid = 0;

		if (solid < consolidateCount)
		{
			solid = consolidateCount;
			return true;
		}
		return false;
	}
};

struct JoyStick
{
	uint8_t xpin;
	uint8_t ypin;
	int16_t xmin = 100;
	int16_t ymin = 100;
	int16_t xmax = 100;
	int16_t ymax = 100;
	bool xinvert;
	bool yinvert;

	JoyStick(uint8_t x, uint8_t y, bool invertX, bool invertY)
	{
		xpin = x;
		ypin = y;
		xinvert = invertX;
		yinvert = invertY;
	}

	int16_t GetX()
	{
		return analogRead(xpin);
	}

	int16_t GetY()
	{
		return analogRead(ypin);
	}

	float GetXFloat()
	{
		xmax = max(xmax, GetX());
		xmin = min(xmin, GetX());
		int16_t delta = xmax - xmin;
		return ((GetX() / (float)delta) * 2.0f - 1.0f) * (xinvert ? -1 : 1);
	}

	float GetYFloat()
	{
		ymax = max(ymax, GetY());
		ymin = min(ymin, GetY());
		int16_t delta = ymax - ymin;
		return ((GetY() / (float)delta) * 2.0f - 1.0f) * (yinvert ? -1 : 1);
	}

	void Init()
	{
		pinMode(xpin, INPUT);
		pinMode(ypin, INPUT);
	}
};



struct Battery
{
	uint32_t nextRead = 0;
	float lastVoltage;
	float lastPercent;
	float GetVoltage()
	{
		if (nextRead > millis()) return lastVoltage;
		int sensorValue = analogRead(ADC_BATTERY);
		// Convert the analog reading (which goes from 0 - 1023) to a voltage (0 - 4.3V):
		lastVoltage = sensorValue * (4.3 / 1023.0);
		return nextRead += 10000;
	}

	float GetPercent()
	{
		float v = GetVoltage();
		float delta = 4.4f - 3.2f;
		float p = (v - 3.2f) / delta * 100;
		if (p > 120) return lastPercent;
		lastPercent = p;
		return p;
	}

	bool LowBattery()
	{
		return GetPercent() < 20;
	}

	bool CriticalBattery()
	{
		return GetVoltage() < 10;
	}
};

class Packet
{
private:
public:
	RemoteType Type;
	Button Stick;
	Button Grip;
	Button Trig;
	Button Menu;
	Button System;
	Button Option1;
	Button Option2;
	JoyStick Joystick;
	Battery Bat;
public:
	Packet(RemoteType type, Button stick, Button grip, Button trig, Button menu, Button sys, Button option1, Button option2, JoyStick joystick);
	void Init();
	size_t printTo(Print& print);
};

#endif

