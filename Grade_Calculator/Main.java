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
        String some_letter;
        double initial_grade;
        double desired_grade;
        double percent;
        double percent_decimal;
        double exam_grade;
        Scanner reader = new Scanner(System.in);

        // typical grading scale
        String[] grading_scale_letter = {"A", "B", "C", "D", "F"};
        Double[] grading_scale_number = {90.00, 80.00, 70.00, 60.00, 0.00};

        // valid arrays
        String[] valid_YN_answers = {"yes", "y", "no", "n"};
        String[] valid_strings = {"a", "b", "c", "d", "f", "all", "some"};
        String[] valid_letters_only = {"a", "b", "c", "d", "f"};
        String[] valid_some_letters = {"a", "b", "c", "d", "f", " ", ","};


        // print grade scale for user to verify
        printGradeScale(grading_scale_letter, grading_scale_number);

        // check with user to see if the the grading scale is accurate
        scale_accuracy = printPrompt("\nIs the scale accurate (y/n)? ", valid_YN_answers, reader);

        // if the user inputs "no" or "n'
        if (("n").equals(scale_accuracy.toLowerCase()) || ("no").equals(scale_accuracy.toLowerCase())) {

            // loops until the user enters an accepted string
            incorrect_letter = printPrompt("Which letter is incorrect (A, B, C, D, F, some, or all)? ", valid_strings, reader);

            // if the string is "some"
            // current bugs = doesn't read space, when asked for the second time if it's correct, typing n results in error
            if (incorrect_letter.toLowerCase().equals("some")) {

                String[] incorrect_letters_array = new String[5];
                int o = 0;
                do {

                    some_letter = "";

                    // loop until the user gives the valid response
                    do {

                        System.out.print("Which letters are incorrect (A, B, C, D, and F)? ");
                        incorrect_letters = reader.next();

                        // go through each character in the string to verify that they're valid
                        for (int n = 0; n < incorrect_letters.length(); n++) {
                            some_letter = Character.toString(incorrect_letters.charAt(n));

                            // if one of the characters is invalid, stop and prompt user for another string
                            if (indexOf(valid_some_letters, some_letter.toLowerCase()) == -1) {
                                System.out.println("Some of the letters you entered are incorrect. Please try again!");
                                o = 0;
                                incorrect_letters_array = new String[5];
                                break;

                                // otherwise
                            } else {

                                // if one of the letters is a valid letter (a, b, c, d, or f), add to array and increment
                                if (indexOf(valid_letters_only, some_letter.toLowerCase()) > -1) {
                                    incorrect_letters_array[o] = some_letter.toLowerCase();
                                    o++;
                                }
                            }

                        }
                    } while (indexOf(valid_some_letters, some_letter.toLowerCase()) == -1);



                    // print each letter and get the correct minimum score
                    for (int p = 0; p < incorrect_letters_array.length; p++) {
                        System.out.println(incorrect_letters_array.length + " " + p);
                        if (incorrect_letters_array[p] != null) {
                            grading_scale_number[indexOf(grading_scale_letter, incorrect_letters_array[p].toUpperCase())] =
                                    checkAndReturn(grading_scale_letter, grading_scale_number, incorrect_letters_array[p].toUpperCase(), reader);
                        }
                    }

                    printGradeScale(grading_scale_letter, grading_scale_number);
                    revised_scale_accuracy = printPrompt("\nIs the scale accurate (y/n)? ", valid_YN_answers, reader);
                } while (!(revised_scale_accuracy.toLowerCase().equals("y") || revised_scale_accuracy.toLowerCase().equals("yes")));

            // otherwise
            } else {

                // if the string is "all"
                if (incorrect_letter.toLowerCase().equals("all")) {

                    // loop until the user is satisfied with the grading scale
                    do {
                        for (int k = 0; k < grading_scale_letter.length; k++) {
                            grading_scale_number[k] = checkAndReturn(grading_scale_letter, grading_scale_number, grading_scale_letter[k], reader);
                        }
                        printGradeScale(grading_scale_letter, grading_scale_number);

                        // loop until the user gives a correct y/n response
                        revised_scale_accuracy = printPrompt("\nIs the scale accurate (y/n)? ", valid_YN_answers, reader);

                    } while (!(revised_scale_accuracy.toLowerCase().equals("y") || revised_scale_accuracy.toLowerCase().equals("yes")));

                // if the string is "A", "B", "C", "D", or "F"
                } else {

                    // loop until the user is satisfied with the grading scale
                    do {
                        grading_scale_number[indexOf(grading_scale_letter, incorrect_letter.toUpperCase())] =
                                checkAndReturn(grading_scale_letter, grading_scale_number, incorrect_letter, reader);
                        printGradeScale(grading_scale_letter, grading_scale_number);

                        // loop until the user gives a correct y/n response
                        revised_scale_accuracy = printPrompt("\nIs the scale accurate (y/n)? ", valid_YN_answers, reader);
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
    private static Boolean checkValidStrings(String inputtedString, String[] validArray) {

        // loop through array of valid inputs and if it matches one of them, return true
        for (int j = 0; j < validArray.length; j++) {
            if (inputtedString.toLowerCase().equals(validArray[j])) {
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
    private static double checkAndReturn(String[] letterArray, Double[] numberArray, String letter, Scanner reader) {
        int current_int = indexOf(letterArray, letter);
        // if the letter is A, there is no previous so it is 100
        Double previous_minimum = (current_int == 0) ? 100.00 : numberArray[current_int - 1];
        // if the letter is F, there is no next so it is 0
        Double next_minimum = (current_int == 4) ? 0.00 : numberArray[current_int + 1];
        Double num_input;

        // loop until the user gives a valid number
        do {
            System.out.print("What is the correct lowest number for " + letter + "? ");
            num_input = reader.nextDouble();
            if ((num_input > previous_minimum) || (num_input < next_minimum)) {
                System.out.println("Not a valid number (either higher than the previous letter or lower than the next)");
            }
        } while ((num_input > previous_minimum) || (num_input < next_minimum));

        return num_input;
    }

    private static String printPrompt(String printString, String[] array, Scanner reader) {
        String user_input;

        // loop until the user gives a correct response
        do {
            System.out.print(printString);
            user_input = reader.next();
        } while (!checkValidStrings(user_input, array));

        return user_input;
    }
}
