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

	Button(uint8_t p)
	{
		pin = p;
	}

	void Init()
	{
		pinMode(pin, INPUT);
	}

	uint8_t GetState() const
	{
		return digitalRead(pin);
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

	JoyStick(uint8_t x, uint8_t y)
	{
		xpin = x;
		ypin = y;
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
		return (GetX() / (float)delta) * 2.0f - 1.0f;
	}

	float GetYFloat()
	{
		ymax = max(ymax, GetY());
		ymin = min(ymin, GetY());
		int16_t delta = ymax - ymin;
		return (GetY() / (float)delta) * 2.0f - 1.0f;
	}

	void Init()
	{
		pinMode(xpin, INPUT);
		pinMode(ypin, INPUT);
	}
};

class Packet
{
private:
public:
	RemoteType Type;
	Button Btn1;
	Button Btn2;
	JoyStick Joystick;
	Button Stick;
	
public:
	Packet(RemoteType type, Button btn1, Button btn2, JoyStick joystick, Button stick);
	void Init();
	size_t printTo(Print& print);
};

#endif

