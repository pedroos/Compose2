#pragma warning(disable:4996) // sprintf

#include <stdio.h>
#include <stdbool.h>

typedef enum RefTypes {
	TypeStr, 
	TypeEx
} RefTypes;

typedef struct Ref {
	RefTypes type;
	void* content;
} Ref;

bool TypedEq(Ref typed1, Ref typed2) {

}

Ref times() {

}

char* ToStr(Ref typed) {
	/*switch (typed.type) {
	case TypeInt:
		return "It is an int";
		break;
	case TypeStr:
		return "It is a string";
		break;
	default:
		return "Unidentified type";
	}*/
}

Ref ToStrInt(int inte) {
	char* str = (char*)malloc(12 * sizeof(char));
	snprintf(str, 12, "%d", inte);
	Ref strR = {
		.type = TypeStr, 
		.content = str
	};
	return strR;
}

typedef void (*FreeStrSig)(char*);

void Free(Ref ref) {
	free(ref);
}

void AssT(bool cond, const char* name) {
	if (!cond) {
		printf(name);
		printf(" FAILED\n");
	}
	else {
		printf(name);
		printf(" ok\n");
	}
}

void AssF(bool cond, const char* name) {
	AssT(!cond, name);
}

void AssEq(Ref actual, Ref expected, const char* name) {
	if (!TypedEq(actual, expected)) {
		printf(name);
		printf(" FAILED: expected '");
		printf(ToStr(expected));
		printf("', got '");
		printf(ToStr(actual));
		printf("'");
	}
	else {
		printf(name);
		printf(" ok\n");
	}
}

void AssEqInt(int actual, int expected, const char* name) {
	if (actual != expected) {
		printf(name);
		printf(" FAILED: expected '");
		printf(ToStrInt(expected).content);
		printf("', got '");
		PrintR(ToStrInt(actual));
		printf("'");
	}
	else {
		printf(name);
		printf(" ok\n");
	}
}

bool StrEq(Ref str1, const char* str2) {
	return strcmp(str1.content, str2) == 0;
}

typedef enum ExTypes {
	WrongRefTypeEx
} ExTypes;

Ref WrongRefTypeException(const char* expected) {
	return Exception(WrongRefTypeEx, expected, "Wrong reference type.");
}

Ref Exception(ExTypes type, const char* expected, const char* msg) {
	const char* msg2;
	if (expected != NULL)
		msg2 = msg != NULL ? msg : "";
	else
		sprintf(msg2, "Expected: %s. %s", expected, msg != NULL ? msg : "");
	Ref ex = {
		.type = type,
		.content = msg2
	};
	printf("Exception thrown (%s): %s", ToStrInt(type).content, msg2);
	return ex;
}

Ref AssEqStr(Ref actual, const char* expected, const char* name) {
	if (actual.type != TypeStr) return Exception(WrongRefTypeEx, ToStrInt(TypeStr).content, NULL);

	if (!StrEq(actual, expected)) {
		printf(name);
		printf(" FAILED: expected '");
		printf("%s", expected);
		printf("', got '");
		printf("%s", (char*)actual.content);
		printf("'");
	}
	else {
		printf(name);
		printf(" ok\n");
	}
	return actual;
}

int AssertTests() {
	AssT(1 == 1, "Assert1");
	AssF(1 == 2, "Assert2");
	AssT("abc" == "abc", "Assert3");
	AssF("abd" == "abc", "Assert4");
	AssEqInt(1, 1, "Assert5");
}

int ToStringTests() {
	AssEqStr(ToStrInt(2), "2", "ToString1");
}

//int TypedTests() {
//	Typed int1 = Int(2);
//	//printf(int1);
//}

int main() {
	AssertTests();
	ToStringTests();

	printf("\n");

	return 0;
}