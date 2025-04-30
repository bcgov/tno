#!/bin/bash

# --- Log Analysis Script for tno-extract-quotes ---

SCRIPT_NAME=$(basename "$0")

# --- Helper Functions ---
print_header() { echo "=================================================="; echo "$1"; echo "=================================================="; }
print_separator() { echo "--------------------------------------------------"; }

print_usage() {
    echo "Usage:"
    echo "  ./${SCRIPT_NAME}          : Automatically finds the latest tno-extract-quotes_*.log file,"
    echo "                           captures new container logs in real-time and appends to this file (creates a new file if not found)."
    echo "                           Press Ctrl+C to stop."
    echo "  ./${SCRIPT_NAME} -f <log file> : Analyzes the specified log file."
}


# --- Analysis Function ---
# Takes the log file path as an argument
analyze_log() {
    local LOG_FILE_TO_ANALYZE=$1

    # --- Input Validation (for analysis mode) ---
    if [[ ! -f "$LOG_FILE_TO_ANALYZE" ]]; then
        echo "Error: File '$LOG_FILE_TO_ANALYZE' not found in analysis mode."
        exit 1
    fi

    echo "Analyzing log file: $LOG_FILE_TO_ANALYZE"
    print_separator

    # --- Analysis ---
    print_header "Overall Summary"
    # Check if file is empty before running head/tail
    if [[ ! -s "$LOG_FILE_TO_ANALYZE" ]]; then
        echo "Log file is empty or does not exist."
        START_TIME="N/A"
        END_TIME="N/A"
        TOTAL_LINES=0
    else
        START_TIME=$(head -n 1 "$LOG_FILE_TO_ANALYZE" | awk '{print $1, $2}')
        END_TIME=$(tail -n 1 "$LOG_FILE_TO_ANALYZE" | awk '{print $1, $2}')
        TOTAL_LINES=$(wc -l < "$LOG_FILE_TO_ANALYZE")
    fi
    echo "Log Start Time: $START_TIME"
    echo "Log End Time: $END_TIME"
    echo "Total Log Lines: $TOTAL_LINES"
    print_separator

    print_header "Service Configuration"
    # Use an array to store configuration items to search for
    CONFIG_ITEMS=(
        "Service configuration - Using LLM:|LLM Usage Configuration"
        "Registering LLM service|LLM Service Registration Log"
        "Initializing rate limiter|Rate Limiter Initialization Log"
    )

    for ITEM in "${CONFIG_ITEMS[@]}"; do
        SEARCH_TERM=$(echo "$ITEM" | cut -d'|' -f1)
        ERROR_MSG=$(echo "$ITEM" | cut -d'|' -f2)
        grep "$SEARCH_TERM" "$LOG_FILE_TO_ANALYZE" | tail -n 1 || echo "Could not find $ERROR_MSG."
    done

    # Extract model names safely
    PRIMARY_MODEL_LINE=$(grep 'LLM service initialized - Primary model:' "$LOG_FILE_TO_ANALYZE" | tail -n 1)
    FALLBACK_MODEL_LINE=$(grep 'Fallback LLM configured - Model:' "$LOG_FILE_TO_ANALYZE" | tail -n 1)
    echo "$PRIMARY_MODEL_LINE" || echo "Could not find primary model initialization log."
    echo "$FALLBACK_MODEL_LINE" || echo "Could not find Fallback LLM configuration log."
    print_separator

    print_header "Kafka Interaction"
    KAFKA_TOPICS=$(grep 'Subscribing to topics:' "$LOG_FILE_TO_ANALYZE" | awk -F': ' '{print $2}' | sort -u)
    RECEIVED_MSGS=$(grep -c 'Received message - Topic:' "$LOG_FILE_TO_ANALYZE")
    COMMITTED_MSGS=$(grep -c 'Message committed from topic:' "$LOG_FILE_TO_ANALYZE")
    PAUSED_COUNT=$(grep -c 'Pausing consumption:' "$LOG_FILE_TO_ANALYZE")
    RESUMED_COUNT=$(grep -c 'Resuming consumption:' "$LOG_FILE_TO_ANALYZE")
    echo "Subscribed Kafka Topics: ${KAFKA_TOPICS:-Not Found}"
    echo "Total Messages Received: $RECEIVED_MSGS"
    echo "Total Messages Committed: $COMMITTED_MSGS"
    echo "Kafka Consumption Paused Count: $PAUSED_COUNT"
    echo "Kafka Consumption Resumed Count: $RESUMED_COUNT"
    print_separator

    print_header "Content Processing & LLM Calls"
    PROCESSED_IDS=$(grep 'Starting to process content ID:' "$LOG_FILE_TO_ANALYZE" | awk '{print $(NF-3)}' | sort -u)
    TOTAL_PROCESSED_COUNT=$(echo "$PROCESSED_IDS" | wc -w)
    TOTAL_PROCESSED_COUNT=${TOTAL_PROCESSED_COUNT:-0} # Ensure it's 0 if empty
    TOTAL_FAILURES=$(grep -c "No quotes extracted from content ID:" "$LOG_FILE_TO_ANALYZE")

    # Function to extract model information
    extract_model_info() {
        local MODEL_TYPE=$1
        local MODEL_LINE=$2
        local MODEL_NAME=""
        local API_URL="N/A"
        local ATTEMPTS=0
        local SUCCESS=0
        local PARSE_FAIL=0
        local OTHER_FAILURES=0

        if [[ -n "$MODEL_LINE" ]]; then
            MODEL_NAME=$(echo "$MODEL_LINE" | awk -F': ' '{print $2}' | awk '{print $1}' | sed 's/,//')
            API_URL_LINE=$(grep "Sending request to LLM API:" "$LOG_FILE_TO_ANALYZE" | grep "$MODEL_NAME" | head -n 1)
            if [[ -n "$API_URL_LINE" ]]; then
                API_URL=$(echo "$API_URL_LINE" | awk '{print $8}')
            fi

            ATTEMPTS=$(grep -c "Attempting LLM request with $MODEL_TYPE model '$MODEL_NAME'" "$LOG_FILE_TO_ANALYZE")
            SUCCESS=$(grep -c "Successfully extracted quotes using $MODEL_TYPE model '$MODEL_NAME'" "$LOG_FILE_TO_ANALYZE")
            PARSE_FAIL=$(grep "Failed to parse LLM response JSON from model '$MODEL_NAME'" "$LOG_FILE_TO_ANALYZE" | wc -l)

            if [[ "$MODEL_TYPE" == "primary" ]]; then
                OTHER_FAILURES=$(grep "Unexpected error during primary LLM call for model '$MODEL_NAME'" "$LOG_FILE_TO_ANALYZE" | wc -l)
            fi

            echo "$MODEL_TYPE Model ($MODEL_NAME @ $API_URL):"
            echo "  Attempts: $ATTEMPTS"
            echo "  Successful Extractions: $SUCCESS"
            echo "  Parse Failures: $PARSE_FAIL"
            if [[ "$MODEL_TYPE" == "primary" ]]; then
                echo "  Other Primary Model Call Errors: $OTHER_FAILURES"
            fi
        else
            if [[ "$MODEL_TYPE" == "primary" ]]; then
                echo "Primary Model: Initialization information not found in log."
            else
                echo "Fallback Model: Not configured or initialization information not found in log."
            fi
        fi
    }

    echo "Total Content IDs Processed: $TOTAL_PROCESSED_COUNT"
    echo "---"
    extract_model_info "primary" "$PRIMARY_MODEL_LINE"
    echo "---"
    extract_model_info "fallback" "$FALLBACK_MODEL_LINE"
    echo "---"
    echo "Total Failures (Primary and Fallback): $TOTAL_FAILURES"
    print_separator

    print_header "Errors & Warnings"
    JSON_ERR_PRIMARY_COUNT=0
    JSON_ERR_FALLBACK_COUNT=0
    RATE_LIMIT_WARN=$(grep -c "Rate limit exceeded" "$LOG_FILE_TO_ANALYZE")

    # Function to extract JSON error information
    show_json_errors() {
        local MODEL_NAME=$1
        local MODEL_TYPE=$2

        if [[ -n "$MODEL_NAME" ]]; then
            local ERROR_COUNT=$(grep "JsonException:" "$LOG_FILE_TO_ANALYZE" -B 1 | grep "model '$MODEL_NAME'" | wc -l)
            echo "${MODEL_TYPE} Model JSON Parse Errors (System.Text.Json.JsonException): $ERROR_COUNT"
            if [[ $ERROR_COUNT -gt 0 ]]; then
                echo "  Error Details (Partial):"
                grep "JsonException: " "$LOG_FILE_TO_ANALYZE" -B 1 | grep "model '$MODEL_NAME'" -A 1 | grep JsonException | head -n 5 | sed 's/^/    /'
            fi
            return $ERROR_COUNT
        else
            echo "${MODEL_TYPE} Model JSON Parse Errors: ${MODEL_TYPE} model information not detected."
            return 0
        fi
    }

    # Extract primary and fallback model names
    PRIMARY_MODEL=$(echo "$PRIMARY_MODEL_LINE" | awk -F': ' '{print $2}' | awk '{print $1}' | sed 's/,//' 2>/dev/null)
    FALLBACK_MODEL=$(echo "$FALLBACK_MODEL_LINE" | awk -F': ' '{print $2}' | awk '{print $1}' | sed 's/,//' 2>/dev/null)

    # Display JSON errors
    show_json_errors "$PRIMARY_MODEL" "PRIMARY_MODEL"
    PARSE_FAIL_PRIMARY=$?

    show_json_errors "$FALLBACK_MODEL" "FALLBACK_MODEL"
    PARSE_FAIL_FALLBACK=$?

    # Display rate limit warnings
    echo "Rate Limit Warning Count: $RATE_LIMIT_WARN"
    if [[ $RATE_LIMIT_WARN -gt 0 ]]; then
        echo "  Warning Details (Partial):"
        grep "Rate limit exceeded" "$LOG_FILE_TO_ANALYZE" | head -n 5 | sed 's/^/    /'
    fi

    # Display JSON parse failure details
    if [[ $PARSE_FAIL_PRIMARY -gt 0 || $PARSE_FAIL_FALLBACK -gt 0 ]]; then
        print_separator
        print_header "JSON Parse Failure Details"
        echo "Note: Only showing snippets of JSON content and exception information recorded in the log during failures."
        grep -A 50 "Failed to parse LLM response JSON from model" "$LOG_FILE_TO_ANALYZE" | grep -E "(Failed to parse LLM response JSON|JSON:|System\.Text\.Json\.JsonException:|--->|at |^\s*})" | sed '/^\s*{/a --- snippet start ---' | sed '/^\s*}/a --- snippet end ---' | sed 's/^/  /'
    fi

    print_separator
    print_header "finished analysis"
}


# --- Main Logic ---
if [[ "$1" == "-f" ]]; then
    if [[ -n "$2" ]]; then
        # --- Analyze Mode ---
        analyze_log "$2"
    else
        echo "Error: -f option requires a log file name."
        print_usage
        exit 1
    fi
elif [[ $# -eq 0 ]]; then
    # --- Capture Mode ---
    print_header "Real-time Log Capture Mode"

    # Prompt user to refresh container
    echo "TIP: For best results, it's recommended to refresh the extract-quotes container first"
    echo "(e.g., run: docker restart tno-extract-quotes)"
    echo "Press Enter to continue..."
    read -r

    # Find latest log file or create a new one
    LATEST_LOG=$(find . -maxdepth 1 -name 'tno-extract-quotes_*.log' -printf '%T@ %p\n' | sort -nr | head -n 1 | cut -d' ' -f2-)

    if [[ -z "$LATEST_LOG" ]]; then
        TARGET_LOG_FILE="./tno-extract-quotes_$(date +%Y%m%d_%H%M%S).log"
        echo "No existing log file found, creating and logging to: $TARGET_LOG_FILE"
        touch "$TARGET_LOG_FILE" || { echo "Error: Could not create log file '$TARGET_LOG_FILE'"; exit 1; }
    else
        TARGET_LOG_FILE=${LATEST_LOG#./}  # Remove ./ prefix if find added it
        echo "Found latest log file, appending logs to: $TARGET_LOG_FILE"
    fi

    echo "running 'docker logs -f tno-extract-quotes'..."
    echo "Logs will be displayed on screen and appended to $TARGET_LOG_FILE"
    echo "Press Ctrl+C to stop capture."
    print_separator

    # Capture logs with stdbuf to ensure immediate line buffering
    stdbuf -oL docker logs -f tno-extract-quotes 2>&1 | stdbuf -oL tee -a "$TARGET_LOG_FILE"

    echo # Add a newline after Ctrl+C
    print_separator
    echo "Log capture stopped. Logs appended to $TARGET_LOG_FILE"

    # Automatic analysis after capture
    print_separator
    echo "Log capture stopped, automatically analyzing log and saving to report.txt..."
    analyze_log "$TARGET_LOG_FILE" > report.txt
    echo "Analysis complete, results saved to report.txt"
    print_separator
else
    echo "Error: Invalid argument '$1'"
    print_usage
    exit 1
fi

exit 0