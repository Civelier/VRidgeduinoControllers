// 
// 
// 

#include "Packet.h"

Packet::Packet(RemoteType type, Button btn1, const Adafruit_MPU6050 mpu) : Btn1(btn1)
{
	Type = type;
	Mpu = mpu;
}

void Packet::Init()
{
	Btn1.Init();
}

size_t Packet::printTo(arduino::Print& print)
{
	/*sensors_event_t a, g, temp;
	Mpu.getEvent(&a, &g, &temp);*/
	print.print(Type);
	print.print(' ');
	print.print(Btn1.GetState());
	/*print.print(' ');
	print.print(a.gyro.x);
	print.print(' ');
	print.print(a.gyro.y);
	print.print(' ');
	print.print(a.gyro.z);
	print.print(' ');
	print.print(a.acceleration.x);
	print.print(' ');
	print.print(a.acceleration.y);
	print.print(' ');
	print.print(a.acceleration.z);*/
}
