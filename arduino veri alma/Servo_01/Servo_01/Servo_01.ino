#include <WiFi.h>
#include <EEPROM.h>
#include <ESPmDNS.h> // Include the mDNS library

// Wi-Fi credentials
const char *ssid = "NetMASTER Uydunet-B97C";        // Replace with your Wi-Fi SSID
const char *password = "32eb97c4"; // Replace with your Wi-Fi password

// TCP server settings
WiFiServer server(12345); // TCP server listening on port 12345

// Pin assignments
#define Led 2
#define servo1 5
#define servo2 18
#define servo3 19
#define servo4 21
#define servo5 22
#define servo6 23

// Servo positions
int AX = 0, BX = 0, CX = 0, DX = 0, EX = 0, FX = 0;
int LX = 2; // LED state

// Initialize EEPROM size
#define EEPROM_SIZE 30

String veri = "0,0,0,0,0,0";

// -----------------------------------------------------------------------
void setup() {
  Serial.begin(9600);
  EEPROM.begin(EEPROM_SIZE);

  pinMode(Led, OUTPUT);
  pinMode(servo1, OUTPUT);
  pinMode(servo2, OUTPUT);
  pinMode(servo3, OUTPUT);
  pinMode(servo4, OUTPUT);
  pinMode(servo5, OUTPUT);
  pinMode(servo6, OUTPUT);

  
  WiFi.begin(ssid, password);// Connect to Wi-Fi
  
  while (WiFi.status() != WL_CONNECTED) 
  {
    delay(1000);
    Serial.print(".");
  }

  Serial.println("Connected to Wi-Fi");
  Serial.print("IP Address: ");
  Serial.println(WiFi.localIP());  // Bu fonksiyon, ESP32'nin aldığı IP adresini yazdırır

  
  server.begin();   // Start the server
  EEpromOku();      // Load saved servo positions from EEPROM

  
  if (MDNS.begin("servo")) // Initialize mDNS
  {
    Serial.println("mDNS responder started");
    digitalWrite(Led, HIGH);
  } 
  else 
  {
    Serial.println("Error starting mDNS");
  }
}

// -----------------------------------------------------------------------
void loop() {
  // Check for incoming client connection
  WiFiClient client = server.accept();
  if (client) {
    Serial.println("Client connected");
    while (client.connected()) 
    {
    digitalWrite(Led, HIGH); 
      if (client.available()) 
      {
        
        veri = client.readStringUntil('\n');// Read data from the client (TCP connection)
        Serial.println("Received: " + veri);

        
        if (veri.length() > 0) {// Parse the received data into servo positions
          // Parse the string in the format AX1500,BX1500,CX1500,DX1500,EX1500,FX1500
          if (veri.indexOf("AX") == 0) {
            AX = veri.substring(2, veri.indexOf(",")).toInt();
          }
          if (veri.indexOf("BX") != -1) {
            BX = veri.substring(veri.indexOf("BX") + 2, veri.indexOf(",", veri.indexOf("BX"))).toInt();
          }
          if (veri.indexOf("CX") != -1) {
            CX = veri.substring(veri.indexOf("CX") + 2, veri.indexOf(",", veri.indexOf("CX"))).toInt();
          }
          if (veri.indexOf("DX") != -1) {
            DX = veri.substring(veri.indexOf("DX") + 2, veri.indexOf(",", veri.indexOf("DX"))).toInt();
          }
          if (veri.indexOf("EX") != -1) {
            EX = veri.substring(veri.indexOf("EX") + 2, veri.indexOf(",", veri.indexOf("EX"))).toInt();
          }
          if (veri.indexOf("FX") != -1) {
            FX = veri.substring(veri.indexOf("FX") + 2).toInt();
          }
          
          
          //EEpromYaz();// After receiving all values, write them to EEPROM
        }

        
        for(int i=0;i<10;i++)// Call function to update the servos based on the received values
        {
        ServoYaz();
        }
        // Send a response to the client (optional)
//        client.println("Servo positions updated.");
//        Serial.println("Client avaibleee");
      }
     Serial.println("Client cccoooonnn"); 
    }
    // Client disconnected
    client.stop();
    Serial.println("Client disconnected");
  }
  ServoYaz();
  LedKontrol();
  
  digitalWrite(Led, HIGH);
  delay(50);
  digitalWrite(Led, LOW);
  delay(100); 
  
  Serial.println("Loopp");
}

// -----------------------------------------------------------------------
void ServoYaz() {
  // Write PWM signals for each servo
  digitalWrite(servo1, HIGH);
  delayMicroseconds(AX); // PWM pulse width for AX
  digitalWrite(servo1, LOW);

  digitalWrite(servo2, HIGH);
  delayMicroseconds(BX); // PWM pulse width for BX
  digitalWrite(servo2, LOW);

  digitalWrite(servo3, HIGH);
  delayMicroseconds(CX); // PWM pulse width for CX
  digitalWrite(servo3, LOW);

  digitalWrite(servo4, HIGH);
  delayMicroseconds(DX); // PWM pulse width for DX
  digitalWrite(servo4, LOW);

  digitalWrite(servo5, HIGH);
  delayMicroseconds(EX); // PWM pulse width for EX
  digitalWrite(servo5, LOW);

  digitalWrite(servo6, HIGH);
  delayMicroseconds(FX); // PWM pulse width for FX
  digitalWrite(servo6, LOW);

  delay(20); // Short delay between updates
}

// -----------------------------------------------------------------------
void LedKontrol() {
  if (LX == 1) {
    LX = 2;
    digitalWrite(Led, HIGH); // LED on
  }

  if (LX == 0) {
    LX = 2;
    digitalWrite(Led, LOW); // LED off
  }
}

// -----------------------------------------------------------------------
void EEpromOku() {
  EEPROM.get(0, AX);
  EEPROM.get(5, BX);
  EEPROM.get(10, CX);
  EEPROM.get(15, DX);
  EEPROM.get(20, EX);
  EEPROM.get(25, FX);
}

// -----------------------------------------------------------------------
void EEpromYaz() {
  EEPROM.put(0, AX);
  EEPROM.put(5, BX);
  EEPROM.put(10, CX);
  EEPROM.put(15, DX);
  EEPROM.put(20, EX);
  EEPROM.put(25, FX);
  EEPROM.commit();
}
