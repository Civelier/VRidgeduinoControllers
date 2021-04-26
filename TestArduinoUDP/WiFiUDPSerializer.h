#ifndef _WiFiUDPSerializer_h
#define _WiFiUDPSerializer_h

#include "Serializer.h"
#include "WiFiUdp.h"
class WiFiUDPSerializer :
    public Serializer
{
public:
	WiFiUDP Udp;
public:
	WiFiUDPSerializer(WiFiUDP udp);
	int AvailableForWrite() override;
	size_t Write(uint8_t b) override;
	size_t Write(const Serializable& serializable) override;
	void Flush() override;
};

#endif