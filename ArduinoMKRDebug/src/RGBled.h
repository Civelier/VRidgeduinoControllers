// RGBled.h

#ifndef _RGBled_h
#define _RGBled_h

#if defined(ARDUINO) && ARDUINO >= 100
	#include "arduino.h"
#else
	#include "WProgram.h"
#endif

#include <SPI.h>
#include <WiFiNINA.h>
#include "utility/wifi_drv.h"


#define GREEN_LED_pin   25
#define BLUE_LED_pin    27
#define RED_LED_pin     26

struct Color
{
	uint8_t R;
	uint8_t G;
	uint8_t B;
};

namespace Colors
{
	const Color Red{ 255, 0, 0 };
	const Color Green{ 0, 255, 0 };
	const Color Blue{ 0, 0, 255 };
	const Color Yellow{ 255, 255, 0 };
	const Color Orange{ 255, 128, 0 };
	const Color LightGreen{ 128, 255, 0 };
	const Color Turquoise{ 0, 255, 128 };
	const Color Aqua{ 0, 255, 255 };
	const Color LightBlue{ 0, 128, 255 };
	const Color Purple{ 127, 0, 255 };
	const Color Pink{ 255, 0, 255 };
	const Color Magenta{ 255, 0, 127 };
	const Color White{ 255, 255, 255 };
	const Color Grey{ 128, 128, 128 };
	const Color Black{ 0, 0, 0 };
}

struct ErrorLight
{
	Color color;
	uint32_t millis;
	bool end;
};

typedef ErrorLight* Pattern;

class RGBClass
{
private:
	void SetLED(ErrorLight light)
	{
		WiFiDrv::analogWrite(RED_LED_pin, light.color.R);
		WiFiDrv::analogWrite(GREEN_LED_pin, light.color.G);
		WiFiDrv::analogWrite(BLUE_LED_pin, light.color.B);
		delay(light.millis);
	}
public:
	void Init()
	{
		WiFiDrv::pinMode(RED_LED_pin, OUTPUT);
		WiFiDrv::pinMode(GREEN_LED_pin, OUTPUT);
		WiFiDrv::pinMode(BLUE_LED_pin, OUTPUT);
	}

	void SetColor(Color color)
	{
		WiFiDrv::analogWrite(RED_LED_pin, color.R);
		WiFiDrv::analogWrite(GREEN_LED_pin, color.G);
		WiFiDrv::analogWrite(BLUE_LED_pin, color.B);
	}

	void PlayPattern(Pattern pattern)
	{
		ErrorLight l;
		int i = 0;
		do
		{
			l = pattern[i++];
			SetLED(l);
		} while (!l.end);
	}

	void SetError(Pattern error)
	{
		while (true)
		{
			for (size_t i = 0; i < 4; i++)
			{
				SetLED(error[i]);
			}
		}
	}
};

extern RGBClass RGB;

struct RGBFlash
{
	Color color;
	uint32_t flashTime;
	uint32_t downTime;
	uint32_t next;
	bool flashing;
	bool on = false;

	RGBFlash(uint32_t flash, uint32_t down, Color col) : flashTime(flash), downTime(down), color(col)
	{

	}

	void Start()
	{
		if (on) return;
		on = true;
		next = millis();
	}

	void Stop()
	{
		on = false;
		RGB.SetColor(Colors::Black);
	}

	void Update()
	{
		if (!on || millis() < next) return;
		
		if (!flashing)
		{
			RGB.SetColor(color);
			next += flashTime;
		}
		else
		{
			RGB.SetColor(Colors::Black);
			next += downTime;
		}
	}
};

#endif

