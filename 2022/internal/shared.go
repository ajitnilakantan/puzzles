package internal

import (
	"fmt"
)

type AOC struct{}

func (t *AOC) Test(log Log) {
	log.Output("log.Output Test\n")
	log.Info("log.Info Test\n")
	fmt.Println("fmt Test")
}

type Log interface {
	Output(fmt string, v ...any)
	Info(fmt string, v ...any)
	Errorf(format string, args ...interface{})
}

type defaultLog struct{}

func (l *defaultLog) Output(format string, v ...any) {
	fmt.Printf(format, v...)
}

func (l *defaultLog) Info(format string, v ...any) {
	fmt.Printf("\033[1;33m")
	fmt.Printf(format, v...)
	fmt.Print("\033[0m")
}

func (l *defaultLog) Errorf(format string, v ...interface{}) {
	fmt.Printf("\033[1;31m")
	fmt.Printf(format, v...)
	fmt.Print("\033[0m")
}

var theLogger Log = nil

func NewLogger() Log {
	theLogger = &defaultLog{}
	return theLogger
}

func GetLogger() Log {
	if theLogger == nil {
		return NewLogger()
	}
	return theLogger
}
