// 
// 
// 

#include "Packet.h"

Packet::Packet(RemoteType type, Button stick, Button grip, Button trig, Button menu, Button sys, Button option1, Button option2, JoyStick joystick) : Stick(stick), Grip(grip), Trig(trig), Menu(menu), System(sys), Option1(option1), Option2(option2), Joystick(joystick)
{
	Type = type;
}

void Packet::Init()
{
	Stick.Init();
	Grip.Init();
	Trig.Init();
	Menu.Init();
	System.Init();
	Option1.Init();
	Option2.Init();
	Joystick.Init();
}

size_t Packet::printTo(Print& print)
{
	/*sensors_event_t a, g, temp;
	Mpu.getEvent(&a, &g, &temp);*/
	print.print(Type);
	print.print(' ');
	print.print(Stick.GetState());
	print.print(' ');
	print.print(Grip.GetState());
	print.print(' ');
	print.print(Trig.GetState());
	print.print(' ');
	print.print(Menu.GetState());
	print.print(' ');
	print.print(System.GetState());
	print.print(' ');
	print.print(Option1.GetState());
	print.print(' ');
	print.print(Option2.GetState());
	print.print(' ');
	print.print(Joystick.GetXFloat());
	print.print(' ');
	print.print(Joystick.GetYFloat());
	print.print(' ');
	print.print(Bat.GetVoltage());
}
