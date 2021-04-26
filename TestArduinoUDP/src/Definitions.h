#ifndef _UDP_Definitions_h
#define _UDP_Definitions_h

#define WIFI_SSID "OpenWrt"
#define WIFI_PASSWORD "Synatronical"

#define DEBUG

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