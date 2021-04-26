// Packet.h

#ifndef _PACKET_h
#define _PACKET_h

#if defined(ARDUINO) && ARDUINO >= 100
	#include "arduino.h"
#else
	#include "WProgram.h"
#endif

#include "Printable.h"
#include "Adafruit_MPU6050.h"

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

class Packet
{
private:
public:
	RemoteType Type;
	Button Btn1;
	Adafruit_MPU6050 Mpu;
public:
	Packet(RemoteType type, Button btn1, Adafruit_MPU6050 mpu);
	void Init();
	size_t printTo(arduino::Print& print);
};

#endif

