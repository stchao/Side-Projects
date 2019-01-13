package com.company;

import java.util.Scanner;

public class Main {

    public static void main(String[] args) {
        calculate();
    }

    private static void calculate() {
        // variables
        String scale_accuracy;
        String revised_scale_accuracy;
        String incorrect_letter;
        String incorrect_letters;
        double initial_grade;
        double desired_grade;
        double percent;
        double percent_decimal;
        double exam_grade;
        Scanner reader = new Scanner(System.in);

        // typical grading scale
        String[] grading_scale_letter = {"A", "B", "C", "D", "F"};
        Double[] grading_scale_number = {90.00, 80.00, 70.00, 60.00, 0.00};

        // print grade scale for user to verify
        printGradeScale(grading_scale_letter, grading_scale_number);

        // check with user to see if the the grading scale is accurate
        System.out.print("\nIs the scale accurate (y/n)? ");
        scale_accuracy = reader.next();

        // if the user inputs "no" or "n'
        if (("n").equals(scale_accuracy.toLowerCase()) || ("no").equals(scale_accuracy.toLowerCase())) {

            // loops until the user enters an accepted string
            do {
                System.out.print("Which letter is incorrect (A, B, C, D, F, some, or all)? ");
                incorrect_letter = reader.next();
            } while (!checkValidStrings(incorrect_letter));

            // if the string is "some"
            if (incorrect_letter.toLowerCase().equals("some")) {

                // loop until the user gives us the accepted string
                do {
                    System.out.print("Which letters are incorrect (A, B, C, D, and F)? ");
                    incorrect_letters = reader.next();
                } while (!checkValidStrings(incorrect_letters));

            // otherwise
            } else {

                // if the string is "all"
                if (incorrect_letter.toLowerCase().equals("all")) {

                    // loop until the user is satisfied with the grading scale
                    do {
                        for (int k = 0; k < grading_scale_letter.length; k++) {
                            System.out.print("What is the correct lowest number for " + grading_scale_letter[k] + "? ");
                            grading_scale_number[k] = checkAndReturn(grading_scale_letter, grading_scale_number, grading_scale_letter[k]);
                        }
                        printGradeScale(grading_scale_letter, grading_scale_number);
                        System.out.print("\nIs the scale accurate (y/n)? ");
                        revised_scale_accuracy = reader.next();
                    } while (!(revised_scale_accuracy.toLowerCase().equals("y") || revised_scale_accuracy.toLowerCase().equals("yes")));

                // if the string is "A", "B", "C", "D", or "F"
                } else {

                    // loop until the user is satisfied with the grading scale
                    do {
                        System.out.print("What is the correct lowest number for " + incorrect_letter + "? ");
                        grading_scale_number[indexOf(grading_scale_letter, incorrect_letter.toUpperCase())] =
                                checkAndReturn(grading_scale_letter, grading_scale_number, incorrect_letter);
                        printGradeScale(grading_scale_letter, grading_scale_number);
                        System.out.print("\nIs the scale accurate (y/n)? ");
                        revised_scale_accuracy = reader.next();
                    } while (!(revised_scale_accuracy.toLowerCase().equals("y") || revised_scale_accuracy.toLowerCase().equals("yes")));
                }
            }
        }

        // get the initial grade
        System.out.print("Enter your current grade (value): ");
        initial_grade = reader.nextDouble();

        // get the desired grade
        System.out.print("Enter your desired grade (value): ");
        desired_grade = reader.nextDouble();

        // get the percent value of the exam
        System.out.print("Enter midterm/final exam's worth (percentage): ");
        percent = reader.nextDouble();

        // calculate percent and the grade
        percent_decimal = percent / 100;
        exam_grade = (desired_grade - ((1 - percent_decimal) * initial_grade)) / percent_decimal;

        // close reader
        reader.close();

        // print result
        System.out.println("To get the desired grade of " + desired_grade + ", you would have to score " +
                exam_grade);

    }

    // function to print grading scale
    private static void printGradeScale(String[] gradeLetter, Double[] gradeNumber) {
        System.out.println("\nGrading Scale:");
        for (int i = 0; i < gradeLetter.length; i++) {
            // if first loop, the max range is 100
            String max_range = i == 0 ? "100.0" : Double.toString(gradeNumber[i - 1] - 1.00);
            System.out.println(gradeLetter[i] + ": " + max_range + " to " + gradeNumber[i]);
        }
    }

    // function to check valid strings
    private static Boolean checkValidStrings(String inputtedString) {
        String[] validInputs = {"a", "b", "c", "d", "f", "all", "some"};

        // loop through array of valid inputs and if it matches one of them, return true
        for (int j = 0; j < validInputs.length; j++) {
            if (inputtedString.toLowerCase().equals(validInputs[j])) {
                return true;
            }
        }

        // return false otherwise
        return false;
    }

    // returns the index of where the object is found
    private static int indexOf(Object[] array, Object item) {
        // checks the array for the item and returns the index it was found
        for (int l = 0; l < array.length; l++) {
            if (item.equals(array[l])) {
                return l;
            }
        }
        // otherwise return -1 for the item not found
        return -1;
    }

    // continues to ask the user until a valid double is given
    private static double checkAndReturn(String[] letterArray, Double[] numberArray, String letter) {
        Scanner reader2 = new Scanner(System.in);
        int previous_minimum_int = indexOf(letterArray, letter);
        // if the letter is A, the max is 100
        Double previous_minimum = (previous_minimum_int == 0) ? 100.00 : numberArray[previous_minimum_int - 1;
        Double num_input;

        // loop until the user gives a valid number
        do {
            System.out.print("What is the correct lowest number for " + letter + "? ");
            num_input = reader2.nextDouble();
        } while (num_input > previous_minimum);
        return num_input;
    }
}
