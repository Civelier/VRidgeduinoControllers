// 
// 
// 

#include "Packet.h"

Packet::Packet(RemoteType type, Button btn1, Button btn2, JoyStick joystick, Button stick) : Btn1(btn1), Btn2(btn2), Joystick(joystick), Stick(stick)
{
	Type = type;
}

void Packet::Init()
{
	Btn1.Init();
	Btn2.Init();
	Joystick.Init();
	Stick.Init();
}

size_t Packet::printTo(Print& print)
{
	/*sensors_event_t a, g, temp;
	Mpu.getEvent(&a, &g, &temp);*/
	print.print(Type);
	print.print(' ');
	print.print(Btn1.GetState());
	print.print(' ');
	print.print(Btn2.GetState());
	print.print(' ');
	print.print(Joystick.GetXFloat());
	print.print(' ');
	print.print(Joystick.GetYFloat());
	print.print(' ');
	print.print(Stick.GetState());
	// read the input on analog pin 0:
	print.print(' ');
	print.print(Bat.GetVoltage());
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
