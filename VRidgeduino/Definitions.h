// Definitions.h

#ifndef _DEFINITIONS_h
#define _DEFINITIONS_h

#if defined(ARDUINO) && ARDUINO >= 100
	#include "arduino.h"
#else
	#include "WProgram.h"
#endif



#define WIFI_SSID "OpenWrt"
#define WIFI_PASSWORD "Synatronical"





//#define DEBUG

#ifdef DEBUG
#define Debug(text) Serial.print(text)
#define DebugLine(text) Serial.println(text)
#define DebugValue(value) Serial.print(#value); Serial.print(": "); Serial.println(value)
#define DebugAssertEqual(variable, value) if (variable != value) \
{\
	Serial.print("Expected \'");\
	Serial.print(#variable);\
	Serial.print("\' to be equal to \'");\
	Serial.print(value);\
	Serial.print("\' but found \'");\
	Serial.print(variable);\
	Serial.println("\'");\
}

#else
#define Debug(text)
#define DebugLine(text)
#define DebugValue(value)
#define DebugAssertEqual(variable, value)
#endif






#endif

