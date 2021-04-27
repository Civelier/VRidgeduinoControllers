/*
 Name:		VRidgeduino.ino
 Created:	4/24/2021 6:17:07 PM
 Author:	civel
*/

#include "Packet.h"
#include "Definitions.h"
#include "RGBled.h"
#include "I2Cdev.h"
#include "MPU6050_6Axis_MotionApps20.h"

#define Throw(error) Debug("Error found in file \'"); Debug(__FILE__); Debug("\' in function \'"); Debug(__FUNCTION__); Debug("\' at line \'"); Debug(__LINE__); Debug("\': "); DebugError(error);\
SetError(error);




enum VRidgeduinoError
{
	Sucess = 0,
	MPUInitFail,	// Orange, Red
	MPUNACK,		// Orange, Yellow
	MPUDmpInitFail, // Yellow, Red
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
	case MPUDmpInitFail:
		DebugLine("MPU dmp initialization failed");
		break;
	case WIFIConnectFail:
		DebugLine("Connection to wifi failed");
		break;
	default:
		break;
	}
}

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

ErrorLight MPUDmpInitFailPattern[] =
{
	ErrorLight{Colors::Yellow, 500},
	ErrorLight{Colors::Black, 500},
	ErrorLight{Colors::Red, 500},
	ErrorLight{Colors::Black, 3500},
};

ErrorLight WIFIConnectFailPattern[] =
{
	ErrorLight{Colors::Red, 500},
	ErrorLight{Colors::Black, 500},
	ErrorLight{Colors::Blue, 500},
	ErrorLight{Colors::Black, 3500},
};

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
	case MPUDmpInitFail:
		pattern = MPUDmpInitFailPattern;
		break;
	case WIFIConnectFail:
		pattern = WIFIConnectFailPattern;
		break;
	default:
		break;
	}
	RGB.SetError(pattern);
}


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

#define INTERRUPT_PIN 4 
volatile bool mpuInterrupt = false;     // indicates whether MPU interrupt pin has gone high
void dmpDataReady() {
	mpuInterrupt = true;
}

// MPU control/status vars
bool dmpReady = false;  // set true if DMP init was successful
uint8_t mpuIntStatus;   // holds actual interrupt status byte from MPU
uint8_t devStatus;      // return status after each device operation (0 = success, !0 = error)
uint16_t packetSize;    // expected DMP packet size (default is 42 bytes)
uint16_t fifoCount;     // count of all bytes currently in FIFO
uint8_t fifoBuffer[64]; // FIFO storage buffer

// orientation/motion vars
Quaternion q;           // [w, x, y, z]         quaternion container
VectorInt16 aa;         // [x, y, z]            accel sensor measurements
VectorInt16 aaReal;     // [x, y, z]            gravity-free accel sensor measurements
VectorInt16 aaWorld;    // [x, y, z]            world-frame accel sensor measurements
VectorFloat gravity;    // [x, y, z]            gravity vector
float euler[3];         // [psi, theta, phi]    Euler angle container
float ypr[3];           // [yaw, pitch, roll]   yaw/pitch/roll container and gravity vector

bool blinkState;

// packet structure for InvenSense teapot demo
uint8_t teapotPacket[14] = { '$', 0x02, 0,0, 0,0, 0,0, 0,0, 0x00, 0x00, '\r', '\n' };

MPU6050 mpu;
Button btn1(0);
Button btn2(1);
Button stick(2);
JoyStick joystick(A1, A2);
Packet packet(RemoteType::RightRemote, btn1, btn2, joystick, stick);
// the setup function runs once when you press reset or power the board
void setup()
{
#ifdef  DEBUG
	Serial.begin(115200);
	delay(1000);
#endif //  DEBUG
	Wire.begin();
	Wire.setClock(400000);
	/*Wire.begin();
	Wire.setClock(400000);
	packet.Init();
	RGB.Init();
	if (!mpu.begin())
	{
		Throw(VRidgeduinoError::MPUInitFail);
	}*/
	RGB.SetColor(Colors::Aqua);
	mpu.initialize();
	//pinMode(INTERRUPT_PIN, INPUT);
	bool connection = mpu.testConnection();
	DebugValue(connection);
	if (!connection)
	{
		Throw(VRidgeduinoError::MPUInitFail);
	}
	RGB.SetColor(Colors::LightBlue);
	if (mpu.dmpInitialize())
	{
		Throw(VRidgeduinoError::MPUDmpInitFail);
	}
	mpu.setXGyroOffset(220);
	mpu.setYGyroOffset(76);
	mpu.setZGyroOffset(-85);
	mpu.setZAccelOffset(1788); // 1688 factory default for my test chip

	RGB.SetColor(Colors::Turquoise);
	// Calibration Time: generate offsets and calibrate our MPU6050

	mpu.CalibrateAccel(30);
	RGB.SetColor(Colors::Purple);
	mpu.CalibrateGyro(30);
	RGB.SetColor(Colors::Pink);
	mpu.PrintActiveOffsets();

	// turn on the DMP, now that it's ready
	DebugLine(F("Enabling DMP..."));
	mpu.setDMPEnabled(true);

	//// enable Arduino interrupt detection
	//Debug(F("Enabling interrupt detection (Arduino external interrupt "));
	//Debug(digitalPinToInterrupt(INTERRUPT_PIN));
	//DebugLine(F(")..."));
	//attachInterrupt(digitalPinToInterrupt(INTERRUPT_PIN), dmpDataReady, RISING);
	mpuIntStatus = mpu.getIntStatus();

	// set our DMP Ready flag so the main loop() function knows it's okay to use it
	DebugLine(F("DMP ready! Waiting for first interrupt..."));
	dmpReady = true;

	// get expected DMP packet size for later comparison
	packetSize = mpu.dmpGetFIFOPacketSize();

	RGB.SetColor(Colors::Yellow);
	ConnectWifi();
	RGB.SetColor(Colors::Green);
	delay(100);
	RGB.SetColor(Colors::Black);
	delay(100);
	RGB.SetColor(Colors::Green);
	delay(100);
	RGB.SetColor(Colors::Black);
	pinMode(LED_BUILTIN, OUTPUT);
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

	while (/*!mpuInterrupt && */fifoCount < packetSize) {
		if (/*mpuInterrupt && */fifoCount < packetSize) {
			// try to get out of the infinite loop 
			fifoCount = mpu.getFIFOCount();
		}
		// other program behavior stuff here
		// .
		// .
		// .
		// if you are really paranoid you can frequently test in between other
		// stuff to see if mpuInterrupt is true, and if so, "break;" from the
		// while() loop to immediately process the MPU data
		// .
		// .
		// .
	}
	// reset interrupt flag and get INT_STATUS byte

	mpuInterrupt = false;
	mpuIntStatus = mpu.getIntStatus();
	//mpuIntStatus = 0;

	// get current FIFO count
	fifoCount = mpu.getFIFOCount();
	if (fifoCount < packetSize) {
		//Lets go back and wait for another interrupt. We shouldn't be here, we got an interrupt from another event
		// This is blocking so don't do it   while (fifoCount < packetSize) fifoCount = mpu.getFIFOCount();
	}
	// check for overflow (this should never happen unless our code is too inefficient)
	else if ((mpuIntStatus & (0x01 << MPU6050_INTERRUPT_FIFO_OFLOW_BIT)) || fifoCount >= 1024) {
		// reset so we can continue cleanly
		mpu.resetFIFO();
		//  fifoCount = mpu.getFIFOCount();  // will be zero after reset no need to ask
		Serial.println(F("FIFO overflow!"));

		// otherwise, check for DMP data ready interrupt (this should happen frequently)
	}
	else if (mpuIntStatus & (0x01 << MPU6050_INTERRUPT_DMP_INT_BIT)) {

		// read a packet from FIFO
		while (fifoCount >= packetSize) { // Lets catch up to NOW, someone is using the dreaded delay()!
			mpu.getFIFOBytes(fifoBuffer, packetSize);
			// track FIFO count here in case there is > 1 packet available
			// (this lets us immediately read more without waiting for an interrupt)
			fifoCount -= packetSize;
		}


		mpuInterrupt = false;
		mpuIntStatus = mpu.getIntStatus();
		mpu.dmpGetQuaternion(&q, fifoBuffer);
		udp.beginPacket(address, PORT);
		packet.printTo(udp);
		udp.print(' ');
		udp.print(q.x);
		udp.print(' ');
		udp.print(q.y);
		udp.print(' ');
		udp.print(q.z);
		udp.print(' ');
		udp.print(q.w);
		udp.endPacket();
	}
	blinkState = !blinkState;
	digitalWrite(LED_BUILTIN, blinkState);
}
