#include <math.h>
int a;
float temperature;
int B=4275;                  //B value of the thermistor
float resistance;

const int LED_CUSTOM = 4;

// the setup function runs once when you press reset or power the board
void setup() {
  // initialize digital pin LED_BUILTIN as an output.
  pinMode(LED_CUSTOM, OUTPUT);
  Serial.begin(115200);
  while (!Serial)
  {
    digitalWrite(LED_CUSTOM, HIGH);
    delay(500);
    digitalWrite(LED_CUSTOM, LOW);
  }
  digitalWrite(LED_CUSTOM, HIGH);   // turn the LED on (HIGH is the voltage level)
}

// the loop function runs over and over again forever
void loop() {
  int lightValue = analogRead(A0);
  int a= analogRead(A3);

  float R = 1023.0/((float)a)-1.0;
  R = 100000.0*R;
  float temperature=1.0/(log(R/100000.0)/B+1/298.15)-273.15;//convert to temperature via datasheet ;

  String lightHead="{'light':";
  String tempHead = ", 'temp':";
  String valuesEnd="}";
  String light=lightHead+lightValue;
  String temp=tempHead+temperature;
  Serial.println(light+temp+valuesEnd);
 //Serial.println(temperature);
  delay(1000);     // wait for a second
}
