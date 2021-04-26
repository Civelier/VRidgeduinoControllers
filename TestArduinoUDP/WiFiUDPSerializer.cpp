#include "WiFiUDPSerializer.h"

WiFiUDPSerializer::WiFiUDPSerializer(WiFiUDP udp)
{
	Udp = udp;
}

int WiFiUDPSerializer::AvailableForWrite()
{
	return Udp.availableForWrite();
}

size_t WiFiUDPSerializer::Write(uint8_t b)
{
	return Udp.write(b);
}

size_t WiFiUDPSerializer::Write(const Serializable& serializable)
{
	return serializable.SendToSerializer(*this);
}

void WiFiUDPSerializer::Flush()
{
	Udp.flush();
}
