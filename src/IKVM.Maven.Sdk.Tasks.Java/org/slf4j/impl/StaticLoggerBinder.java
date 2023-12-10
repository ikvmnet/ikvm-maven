package org.slf4j.impl;

import org.slf4j.*;
import org.slf4j.spi.*;

import IKVM.Maven.Sdk.Tasks.Java.*;

public final class StaticLoggerBinder implements LoggerFactoryBinder  {
    
    public static String REQUESTED_API_VERSION = "1.7";
    static final StaticLoggerBinder SINGLETON = new StaticLoggerBinder();

    final ILoggerFactory loggerFactory;

    private StaticLoggerBinder() {
        loggerFactory = new AdapterLoggerFactory(AdapterLoggerFactory.LoggerFactory);
    }

    public static StaticLoggerBinder getSingleton() {
        return SINGLETON;
    }

    @Override
    public ILoggerFactory getLoggerFactory() {
        return loggerFactory;
    }

    @Override
    public String getLoggerFactoryClassStr() {
        return AdapterLoggerFactory.class.getName();
    }

}
