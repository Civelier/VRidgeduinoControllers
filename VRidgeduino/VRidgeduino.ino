/*
 Name:		VRidgeduino.ino
 Created:	4/24/2021 6:17:07 PM
 Author:	civel
*/

#include <Adafruit_I2CRegister.h>
#include <Adafruit_SPIDevice.h>
#include <Adafruit_I2CDevice.h>
#include <Adafruit_BusIO_Register.h>
#include <Adafruit_Sensor.h>
#include <Adafruit_MPU6050.h>
#include "Packet.h"
#include <Wire.h>
#include <ArduinoLowPower.h>
#include "Definitions.h"
#include <SPI.h>
#include <WiFiNINA.h>
#include "utility/wifi_drv.h"

#define Throw(error) Debug("Error found in file \'"); Debug(__FILE__); Debug("\' in function \'"); Debug(__FUNCTION__); Debug("\' at line \'"); Debug(__LINE__); Debug("\': "); DebugError(error);\
RGB.SetError(error)


#define GREEN_LED_pin   25
#define BLUE_LED_pin    27
#define RED_LED_pin     26


enum VRidgeduinoError
{
	Sucess = 0,
	MPUInitFail,	// Orange, Red
	MPUNACK,		// Orange, Yellow
	MPURegNACK,		// Orange, Blue
	WIFIConnectFail,// Red, Blue
};

void DebugError(VRidgeduinoError error)
{
	switch (error)
	{
	case Sucess:
		break;
	case MPUInitFail:
		DebugLine("MPU initialization failed");
		break;
	case MPUNACK:
		DebugLine("MPU did not ack, verify connections");
		break;
	case MPURegNACK:
		DebugLine("MPU registry did not ack, is the device working properly");
		break;
	case WIFIConnectFail:
		DebugLine("Connection to wifi failed");
		break;
	default:
		break;
	}
}

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
};

typedef ErrorLight* Pattern;

ErrorLight MPUInitFailPattern[] = 
{
	ErrorLight{Colors::Orange, 500},
	ErrorLight{Colors::Black, 500},
	ErrorLight{Colors::Red, 500},
	ErrorLight{Colors::Black, 3500},
};

ErrorLight MPUNACKPattern[] =
{
	ErrorLight{Colors::Orange, 500},
	ErrorLight{Colors::Black, 500},
	ErrorLight{Colors::Yellow, 500},
	ErrorLight{Colors::Black, 3500},
};

ErrorLight MPURegNACKPattern[] =
{
	ErrorLight{Colors::Orange, 500},
	ErrorLight{Colors::Black, 500},
	ErrorLight{Colors::Blue, 500},
	ErrorLight{Colors::Black, 3500},
};

ErrorLight WIFIConnectFailPattern[] =
{
	ErrorLight{Colors::Red, 500},
	ErrorLight{Colors::Black, 500},
	ErrorLight{Colors::Blue, 500},
	ErrorLight{Colors::Black, 3500},
};


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

	void SetError(VRidgeduinoError error)
	{
		Pattern pattern;

		switch (error)
		{
		case Sucess:
			break;
		case MPUInitFail:
			pattern = MPUInitFailPattern;
			break;
		case MPUNACK:
			pattern = MPUNACKPattern;
			break;
		case MPURegNACK:
			pattern = MPURegNACKPattern;
			break;
		case WIFIConnectFail:
			pattern = WIFIConnectFailPattern;
			break;
		default:
			break;
		}

		while (true)
		{
			for (size_t i = 0; i < 4; i++)
			{
				SetLED(pattern[i]);
			}
		}
	}
};

RGBClass RGB;

IPAddress address(192, 168, 2, 11);

#define PORT 7000

WiFiUDP udp;

void ConnectWifi()
{
	//WiFi.setTimeout(10000);
	if (!WiFi.begin(WIFI_SSID, WIFI_PASSWORD))
	{
		Throw(VRidgeduinoError::WIFIConnectFail);
	}
	udp.begin(PORT);
}


Adafruit_MPU6050 mpu;
Button btn1(0);
Packet packet(RemoteType::RightRemote, btn1, mpu);
// the setup function runs once when you press reset or power the board
void setup()
{
#ifdef  DEBUG
	Serial.begin(115200);
	delay(1000);
#endif //  DEBUG
	/*Wire.begin();
	Wire.setClock(400000);
	packet.Init();
	RGB.Init();
	if (!mpu.begin())
	{
		Throw(VRidgeduinoError::MPUInitFail);
	}*/
	RGB.SetColor(Colors::Yellow);
	ConnectWifi();
	RGB.SetColor(Colors::Green);
	delay(500);
	RGB.SetColor(Colors::Aqua);
	delay(500);
	RGB.SetColor(Colors::Pink);
	delay(500);
	RGB.SetColor(Colors::Black);
}

// the loop function runs over and over again until power down or reset
void loop()
{
	/*digitalWrite(GREEN_LED_pin, HIGH);
	sensors_event_t a, g, temp;
	mpu.getEvent(&a, &g, &temp);



	Debug("Acceleration X: ");
	Debug(a.acceleration.x);
	Debug(", Y: ");
	Debug(a.acceleration.y);
	Debug(", Z: ");
	Debug(a.acceleration.z);
	DebugLine(" m/s^2");

	Debug("Rotation X: ");
	Debug(g.gyro.x);
	Debug(", Y: ");
	Debug(g.gyro.y);
	Debug(", Z: ");
	Debug(g.gyro.z);
	DebugLine(" rad/s");


	Debug("Temperature: ");
	Debug(temp.temperature);
	DebugLine(" degC");

	DebugLine("");*/

	udp.beginPacket(address, PORT);
	packet.printTo(udp);
	udp.endPacket();

	delay(10);
}
